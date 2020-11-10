using Windows.UI.Xaml.Controls;
using KioskApp.Controls;
using KioskApp.Ek;
using KioskBrains.Kiosk.Core.Application;
using KioskBrains.Kiosk.Core.Components.Initialization;

namespace KioskApp.CoreExtension.Application
{
    public class KioskApplicationViewProvider : IKioskApplicationViewProvider
    {
        public UserControl GetInitializationView(ComponentInitializationLog initializationLog)
        {
            return new InitializationView(initializationLog);
        }

        public UserControl GetErrorPageView(string errorMessage)
        {
            return new ErrorControl(errorMessage);
        }

        public KioskApplicationViews GetApplicationViews()
        {
            var applicationView = new KioskApplicationView(KioskApplication.Current);
            return new KioskApplicationViews()
            {
                ApplicationView = applicationView,
                ModalLayer = applicationView.GetModalLayer(),
            };
        }

        public UserControl GetMainPageView()
        {
            return new EkApplicationView();
        }
    }
}