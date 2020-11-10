using Windows.UI.Xaml.Controls;
using KioskBrains.Kiosk.Core.Components.Initialization;

namespace KioskBrains.Kiosk.Core.Application
{
    public interface IKioskApplicationViewProvider
    {
        UserControl GetInitializationView(ComponentInitializationLog initializationLog);

        UserControl GetErrorPageView(string errorMessage);

        KioskApplicationViews GetApplicationViews();

        UserControl GetMainPageView();
    }
}