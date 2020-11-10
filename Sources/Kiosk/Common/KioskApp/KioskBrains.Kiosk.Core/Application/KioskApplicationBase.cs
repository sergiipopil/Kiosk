using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Helpers;
using KioskBrains.Common.KioskConfiguration;
using KioskBrains.Common.KioskState;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Components;
using KioskBrains.Kiosk.Core.Components.Contracts;
using KioskBrains.Kiosk.Core.Components.Initialization;
using KioskBrains.Kiosk.Core.Components.Operations;
using KioskBrains.Kiosk.Core.Inactivity;
using KioskBrains.Kiosk.Core.Logging;
using KioskBrains.Kiosk.Core.Modals;
using KioskBrains.Kiosk.Core.ServerSync;
using KioskBrains.Kiosk.Core.Settings;
using KioskBrains.Kiosk.Helpers.Applications;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskBrains.Kiosk.Core.Application
{
    public abstract class KioskApplicationBase : ComponentBase, IKioskApplicationContract
    {
        #region Component

        public override bool IsStateMonitorable => false;

        protected override Type[] GetSupportedContracts()
        {
            return new[]
            {
                typeof(IKioskApplicationContract),
            };
        }

        protected override Task<ComponentInitializeResponse> InitializeAsync(ComponentInitializeRequest request, ComponentOperationContext context)
        {
            Status.SetSelfStatus(ComponentStatusCodeEnum.Ok, null);
            return Task.FromResult(ComponentInitializeResponse.GetSuccess());
        }

        #endregion

        #region IKioskApplicationContract

        public KioskApplicationState State { get; } = new KioskApplicationState();

        #endregion

        #region UWP Application Events

        private bool _isLoggingInitialized;
        private bool _isUwpApplicationInitialized;

        public void UwpApplication_Resuming(object sender, object args)
        {
            Log.Warning(LogContextEnum.Application, "Resuming.");
        }

        public async void UwpApplication_Suspending(object sender, SuspendingEventArgs args)
        {
            var deferral = args.SuspendingOperation.GetDeferral();
            Log.Info(LogContextEnum.Application, "Suspending.");
            await OnApplicationExitAsync();
            deferral.Complete();
        }

        public void UwpApplication_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs args)
        {
            // works only if raised from non-UI thread
            args.Handled = true;
            Log.Error(LogContextEnum.Application, "Unhandled exception!", args.Exception);
            OnUnhandledException();
        }

        public void UwpApplication_OnLaunched(LaunchActivatedEventArgs e)
        {
            if (!_isLoggingInitialized)
            {
                // logging initialization
                JsonDefaultSettings.Initialize();
                Log.Initialize(new Logger(), VersionHelper.AppVersionString);
                _isLoggingInitialized = true;
            }

            var currentView = CoreApplication.GetCurrentView();

            Log.Info(LogContextEnum.Application, "Launched.", new
            {
                Kind = e.Kind.ToString(),
                PreviousExecutionState = e.PreviousExecutionState.ToString(),
                ViewCount = CoreApplication.Views.Count,
                IsCurrentViewMain = currentView.IsMain,
            });

            if (!_isUwpApplicationInitialized)
            {
#if DEBUG
                // enter fullscreen (dev mode only)
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
                var applicationView = ApplicationView.GetForCurrentView();
                applicationView.TryEnterFullScreenMode();
#endif

                // helper initialization
                ThreadHelper.Initialize(currentView.Dispatcher);

#pragma warning disable 4014
                StartKioskApplicationAsync();
#pragma warning restore 4014

                _isUwpApplicationInitialized = true;
            }
            else
            {
                Log.Error(LogContextEnum.Application, "Application is initialized already.");
            }

            Window.Current.Activate();
        }

        #endregion

        #region Startup

        private async Task StartKioskApplicationAsync()
        {
            try
            {
                // block updates during initialization
                await KioskAppNotReadyForUpdateSign.SetAsync();

                State.ApplicationState = KioskApplicationStateEnum.Initializing;

                _applicationViewProvider = GetApplicationViewProvider();
                Assure.CheckFlowState(_applicationViewProvider != null, $"'{nameof(GetApplicationViewProvider)}' returned null.");

                // set initialization view
                SetWindowView(GetInitializationView(ComponentManager.Current.InitializationLog));

#if !DEBUG
                await Task.Delay(ApplicationInitializationDelay);
#endif

                // register itself as component
                ComponentManager.Current.RegisterContractInternal<IKioskApplicationContract>(
                    "KioskApplication",
                    new ComponentContractInfo(this, Status, State));

                // register supported components
                var supportedComponentTypes = new List<Type>()
                {
                    // default components
                    typeof(KioskAppSettings),
                    typeof(ServerSyncService),
                };
                var supportedAppComponentTypes = GetSupportedAppComponents();
                if (supportedAppComponentTypes != null)
                {
                    supportedComponentTypes.AddRange(supportedAppComponentTypes);
                }

                ComponentManager.Current.Initialize(supportedComponentTypes);

                // init app settings component
                await ComponentManager.Current.RegisterAndInitializeAsync(
                    new ComponentConfiguration()
                    {
                        ComponentRole = CoreComponentRoles.KioskAppSettings,
                        ComponentName = nameof(KioskAppSettings),
                        Settings = null,
                    });

                var kioskAppSettingsComponent = ComponentManager.Current.GetComponent<IKioskAppSettingsContract>(CoreComponentRoles.KioskAppSettings);
                if (!kioskAppSettingsComponent.Status.IsOperational())
                {
                    throw new KioskAppInitializationException($"{kioskAppSettingsComponent.FullName}: {kioskAppSettingsComponent.Status.Message}");
                }

                // init core components
                await ComponentManager.Current.RegisterAndInitializeAsync(
                    new ComponentConfiguration()
                    {
                        ComponentRole = CoreComponentRoles.ServerSyncService,
                        ComponentName = nameof(ServerSyncService),
                        Settings = null,
                    });

                // register app specific components
                await ComponentManager.Current.RegisterAndInitializeAsync(kioskAppSettingsComponent.State.KioskConfiguration.AppComponents);

                // component initialization is completed - prepare and run app UI
                // application view
                var applicationViews = GetApplicationViews();
                SetWindowView(applicationViews.ApplicationView);

                // initialize common UI managers
                ModalManager.Current.Initialize(applicationViews.ModalLayer, GetModalViewProvider());

                // initialize inactivity behavior
                InactivityManager.Current.Initialize(applicationViews.ApplicationView, GetInactivityViewProvider());

                await GotoMainPageAsync();

                await OnLaunchedAsync();
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Application, "Initialization.", ex);

                // allow updates if initialization fails
                await KioskAppNotReadyForUpdateSign.UnsetAsync();

                State.ApplicationState = KioskApplicationStateEnum.Error;

                // todo: doesn't show ex.Message (should be shown in admin/service panel - KioskOperability 'Operability' component)
                SetWindowView(GetErrorPageView(ex.Message));
            }
        }

        public void SetWindowView(UserControl view)
        {
            var currentView = Window.Current.Content as IDisposable;
            currentView?.Dispose();
            Window.Current.Content = view;
        }

        private IKioskApplicationViewProvider _applicationViewProvider;

        protected abstract Type[] GetSupportedAppComponents();

        protected abstract IKioskApplicationViewProvider GetApplicationViewProvider();

        protected abstract IModalViewProvider GetModalViewProvider();

        protected abstract IInactivityViewProvider GetInactivityViewProvider();

        /// <summary>
        /// Delay before initialization in order to wait for Restart-Computer in case of auto-update.
        /// </summary>
        protected abstract TimeSpan ApplicationInitializationDelay { get; }

        protected virtual Task OnLaunchedAsync()
        {
            return Task.CompletedTask;
        }

        private UserControl GetErrorPageView(string errorMessage)
        {
            var errorPageView = _applicationViewProvider.GetErrorPageView(errorMessage);
            Assure.CheckFlowState(errorPageView != null, $"'{nameof(IKioskApplicationViewProvider)}.{nameof(IKioskApplicationViewProvider.GetErrorPageView)}' returned null.");
            return errorPageView;
        }

        private UserControl GetInitializationView(ComponentInitializationLog initializationLog)
        {
            var initizalitionView = _applicationViewProvider.GetInitializationView(initializationLog);
            Assure.CheckFlowState(initizalitionView != null, $"'{nameof(IKioskApplicationViewProvider)}.{nameof(IKioskApplicationViewProvider.GetInitializationView)}' returned null.");
            return initizalitionView;
        }

        private KioskApplicationViews GetApplicationViews()
        {
            var applicationViews = _applicationViewProvider.GetApplicationViews();
            Assure.CheckFlowState(applicationViews != null, $"'{nameof(IKioskApplicationViewProvider)}.{nameof(IKioskApplicationViewProvider.GetApplicationViews)}' returned null.");
            Assure.CheckFlowState(applicationViews.ApplicationView != null, $"'{nameof(KioskApplicationViews)}.{nameof(KioskApplicationViews.ApplicationView)}' is null.");
            Assure.CheckFlowState(applicationViews.ModalLayer != null, $"'{nameof(KioskApplicationViews)}.{nameof(KioskApplicationViews.ModalLayer)}' is null.");
            return applicationViews;
        }

        private UserControl GetMainPageView()
        {
            var mainPageView = _applicationViewProvider.GetMainPageView();
            Assure.CheckFlowState(mainPageView != null, $"'{nameof(IKioskApplicationViewProvider)}.{nameof(IKioskApplicationViewProvider.GetMainPageView)}' returned null.");
            return mainPageView;
        }

        #endregion

        #region ContentView

        private UserControl _contentView;

        public UserControl ContentView
        {
            get => _contentView;
            set => SetProperty(ref _contentView, value);
        }

        #endregion

        #region IsContentViewLoading

        private bool _isContentViewLoading;

        public bool IsContentViewLoading
        {
            get => _isContentViewLoading;
            set => SetProperty(ref _isContentViewLoading, value);
        }

        #endregion

        #region Navigation

        public Task<bool> SetContentViewAsync(Func<Task<UserControl>> contentViewBuilder)
        {
            Assure.ArgumentNotNull(contentViewBuilder, nameof(contentViewBuilder));

            var taskCompletionSource = new TaskCompletionSource<bool>();
            ThreadHelper.RunInUiThreadAsync(async () =>
            {
                try
                {
                    IsContentViewLoading = true;

                    var contentView = await contentViewBuilder();
                    Assure.CheckFlowState(contentView != null, $"'{nameof(contentViewBuilder)}' returned null.");

                    // dispose current one
                    var disposableView = ContentView as IDisposable;
                    disposableView?.Dispose();

                    ContentView = contentView;

                    taskCompletionSource.SetResult(true);
                }
                catch (Exception ex)
                {
                    Log.Error(LogContextEnum.Application, $"'{nameof(SetContentViewAsync)}' failed.", ex);
                    taskCompletionSource.SetResult(false);
                }
                finally
                {
                    IsContentViewLoading = false;
                }
            });
            return taskCompletionSource.Task;
        }

        public async Task GotoMainPageAsync()
        {
            var isSuccess = await SetContentViewAsync(async () =>
            {
                await ModalManager.Current.ClearModals();
                return GetMainPageView();
            });
            if (!isSuccess)
            {
                SetWindowView(GetErrorPageView("Main Page couldn't be loaded."));
            }

            // allow updates on the main page (even if it's loaded with error)
            await KioskAppNotReadyForUpdateSign.UnsetAsync();

            State.ApplicationState = KioskApplicationStateEnum.MainPage;
        }

        public async Task OnApplicationExitAsync()
        {
            await DelayHelper.WaitForAsyncFileIoFinished();
        }

        public void OnUnhandledException()
        {
#pragma warning disable 4014
            GotoMainPageAsync();
#pragma warning restore 4014
        }

        #endregion
    }
}