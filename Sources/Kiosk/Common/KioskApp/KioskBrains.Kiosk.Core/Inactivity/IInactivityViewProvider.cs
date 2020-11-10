using Windows.UI.Xaml.Controls;

namespace KioskBrains.Kiosk.Core.Inactivity
{
    public interface IInactivityViewProvider
    {
        UserControl GetInactivityConfirmationModalWithStartedCountdown(InactivityConfirmationModalModel inactivityConfirmationModalModel);
    }
}