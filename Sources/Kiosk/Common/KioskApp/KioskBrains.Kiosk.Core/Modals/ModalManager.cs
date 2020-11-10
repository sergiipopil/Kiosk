using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskBrains.Kiosk.Core.Modals
{
    public class ModalManager
    {
        #region Singleton

        public static ModalManager Current { get; } = new ModalManager();

        private ModalManager()
        {
        }

        #endregion

        private Panel _modalLayer;

        private IModalViewProvider _modalViewProvider;

        public void Initialize(Panel modalLayer, IModalViewProvider modalViewProvider)
        {
            Assure.ArgumentNotNull(modalLayer, nameof(modalLayer));
            Assure.ArgumentNotNull(modalViewProvider, nameof(modalViewProvider));

            _modalLayer = modalLayer;
            _modalViewProvider = modalViewProvider;
        }

        private void CheckIfInitialized()
        {
            Assure.CheckFlowState(_modalViewProvider != null, $"'{nameof(ModalManager)}' is not initialized. Run '{nameof(Initialize)}' first.");
        }

        public async Task ShowModalAsync(ModalArgs modalArgs)
        {
            Assure.ArgumentNotNull(modalArgs, nameof(modalArgs));
            CheckIfInitialized();

            try
            {
                await ThreadHelper.RunInUiThreadAsync(() =>
                    {
                        var modalContext = new ModalContext();
                        var modalContent = modalArgs.ModalContentProvider(modalContext);
                        Assure.CheckFlowState(modalContent != null, $"'{nameof(ModalArgs)}.{nameof(ModalArgs.ModalContentProvider)}' has returned null.");

                        var modalContentContainer = _modalViewProvider.GetModalContentContainer(modalContent, () => modalContext.OnCloseModalRequest(), modalArgs.ModalTitle, modalArgs.CenterTitleHorizontally, modalArgs.ShowCancelButton);
                        Assure.CheckFlowState(modalContentContainer != null, $"'{nameof(IModalViewProvider)}.{nameof(IModalViewProvider.GetModalContentContainer)}' has returned null.");

                        _modalLayer.Children.Add(modalContentContainer);

                        modalContext.OnCloseModalRequest = () =>
                            {
                                _modalLayer.Children.Remove(modalContentContainer);
                                (modalContent as IDisposable)?.Dispose();
                            };
                        // check is closed on construction phase
                        if (modalContext.IsEarlyClosingRequested)
                        {
                            modalContext.OnCloseModalRequest();
                        }
                    });
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Application, $"'{nameof(ShowModalAsync)}' has failed.", ex);
            }
        }

        public async Task ClearModals()
        {
            try
            {
                await ThreadHelper.RunInUiThreadAsync(() => { _modalLayer.Children.Clear(); });
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Application, $"'{nameof(ClearModals)}' has failed.", ex);
            }
        }
    }
}