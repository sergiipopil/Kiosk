using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using KioskApp.CoreExtension.Application;

namespace KioskApp
{
    sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // application events
            Resuming += KioskApplication.Current.UwpApplication_Resuming;
            Suspending += KioskApplication.Current.UwpApplication_Suspending;
            UnhandledException += KioskApplication.Current.UwpApplication_UnhandledException;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            KioskApplication.Current.UwpApplication_OnLaunched(e);
        }
    }
}