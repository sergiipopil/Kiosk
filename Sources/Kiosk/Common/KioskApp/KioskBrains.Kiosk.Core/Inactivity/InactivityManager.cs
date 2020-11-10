using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Modals;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskBrains.Kiosk.Core.Inactivity
{
    /// <summary>
    /// Uses 3 ways to catch user activity:
    /// - pointer Entered/Exited events - works well ONLY with touch inputs
    /// - KeyDown events - works when user uses keyboard (including touch keyboard - text input should be in focus)
    /// - custom interaction - triggered by code explicitly
    /// </summary>
    public class InactivityManager
    {
        #region Singleton

        public static InactivityManager Current { get; } = new InactivityManager();

        private InactivityManager()
        {
        }

        #endregion

        private UserControl _rootControl;

        private IInactivityViewProvider _inactivityViewProvider;

        private void CheckIfInitialized()
        {
            Assure.CheckFlowState(_inactivityViewProvider != null, $"'{nameof(InactivityManager)}' is not initialized. Run '{nameof(Initialize)}' first.");
        }

        public void Initialize(UserControl rootControl, IInactivityViewProvider inactivityViewProvider)
        {
            Assure.ArgumentNotNull(rootControl, nameof(rootControl));
            Assure.ArgumentNotNull(inactivityViewProvider, nameof(inactivityViewProvider));

            _rootControl = rootControl;
            _inactivityViewProvider = inactivityViewProvider;

            // touch events
            _rootControl.PointerEntered += OnScreenTouched;
            _rootControl.PointerExited += OnScreenTouched;

            // keyboard events
            _rootControl.KeyDown += OnKeyEntered;

            RunInactivityProcessing();
        }

        private void OnScreenTouched(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            UpdateLastActivityTimestamp();
        }

        private void OnKeyEntered(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            UpdateLastActivityTimestamp();
        }

        private void OnCustomInteraction()
        {
            UpdateLastActivityTimestamp();
        }

        private void UpdateLastActivityTimestamp()
        {
            _lastActivityTimestamp = DateTime.Now;
            ResetTracking();
        }

        // no need to sync since assignment is atomic
        private DateTime _lastActivityTimestamp = DateTime.Now;

        /// <param name="source">For readability.</param>
        public void RegisterCustomInteraction(InactivityCustomInteractionSourceEnum source)
        {
            OnCustomInteraction();
        }

        private static readonly TimeSpan InactivityCheckInterval = TimeSpan.FromSeconds(1);

        private readonly object _inactivityProcessingLock = new object();

        private InactivityBehavior _inactivityBehavior;

        private Action _userAbsenceHandler;

        private InactivityConfirmationModalModel _currentInactivityModalModel;

        private void RunInactivityProcessing()
        {
            ThreadHelper.RunInNewThreadAsync(async () =>
                {
                    try
                    {
                        while (true)
                        {
                            await Task.Delay(InactivityCheckInterval);

                            InactivityBehavior activeBehavior;
                            bool isInactivityModalActive;
                            lock (_inactivityProcessingLock)
                            {
                                activeBehavior = _inactivityBehavior;
                                isInactivityModalActive = _currentInactivityModalModel != null;
                            }
                            if (activeBehavior == null
                                || isInactivityModalActive)
                            {
                                continue;
                            }

                            var timeSinceLastActivity = DateTime.Now - _lastActivityTimestamp;
                            if (timeSinceLastActivity.TotalSeconds > activeBehavior.DelayBeforeModalSecs)
                            {
                                await ShowInactivityConfirmationModal();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(LogContextEnum.Application, "Inactivity processing has failed.", ex);
                    }
                });
        }

        private Task ShowInactivityConfirmationModal()
        {
            return ModalManager.Current.ShowModalAsync(new ModalArgs(
                "InactivityConfirmationModal_Question",
                modalContext =>
                    {
                        lock (_inactivityProcessingLock)
                        {
                            // check if tracking has been stopped in parallel
                            if (_inactivityBehavior == null)
                            {
                                throw new OperationCanceledException($"Inactivity modal has been interrupted by '{nameof(StopTracking)}'.");
                            }

                            _currentInactivityModalModel = new InactivityConfirmationModalModel(modalContext, _inactivityBehavior.ModalDurationSecs, OnConfirmationModalCountdownRunOut);
                            var modal = _inactivityViewProvider.GetInactivityConfirmationModalWithStartedCountdown(_currentInactivityModalModel);
                            Assure.CheckFlowState(modal != null, $"'{nameof(IInactivityViewProvider)}.{nameof(IInactivityViewProvider.GetInactivityConfirmationModalWithStartedCountdown)}' has returned null.");
                            return modal;
                        }
                    },
                centerTitleHorizontally: true,
                showCancelButton: false));
        }

        private void OnConfirmationModalCountdownRunOut()
        {
            lock (_inactivityProcessingLock)
            {
                // check if tracking has been stopped in parallel
                if (_inactivityBehavior == null)
                {
                    Log.Warning(LogContextEnum.Application, $"'{nameof(OnConfirmationModalCountdownRunOut)}' - abnormal case.");
                    return;
                }

                ResetTracking();

                // run handler outside the lock
                ThreadHelper.RunInNewThreadAsync(() =>
                    {
                        _userAbsenceHandler();
                        return Task.CompletedTask;
                    });
            }
        }

        private void CloseConfirmationModal()
        {
            lock (_inactivityProcessingLock)
            {
                if (_currentInactivityModalModel != null)
                {
                    _currentInactivityModalModel.IgnoreCountdown();
                    _currentInactivityModalModel.ModalContext.CloseModalAsync();
                    _currentInactivityModalModel = null;
                }
            }
        }

        public void ResetTracking()
        {
            // fast check if modal is opened
            if (_currentInactivityModalModel != null)
            {
                CloseConfirmationModal();
            }
        }

        public void StopTracking()
        {
            lock (_inactivityProcessingLock)
            {
                CloseConfirmationModal();
                _inactivityBehavior = null;
                _userAbsenceHandler = null;
            }
        }

        public void StartTracking(InactivityBehavior inactivityBehavior, Action userAbsenceHandler)
        {
            CheckIfInitialized();

            lock (_inactivityProcessingLock)
            {
                StopTracking();
                UpdateLastActivityTimestamp();

                if (inactivityBehavior.IsDisabled)
                {
                    return;
                }

                _inactivityBehavior = inactivityBehavior;
                _userAbsenceHandler = userAbsenceHandler;
            }
        }
    }
}