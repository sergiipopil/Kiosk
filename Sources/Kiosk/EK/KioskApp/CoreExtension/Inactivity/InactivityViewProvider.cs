using Windows.UI.Xaml.Controls;
using KioskBrains.Kiosk.Core.Inactivity;

namespace KioskApp.CoreExtension.Inactivity
{
    public class InactivityViewProvider : IInactivityViewProvider
    {
        public UserControl GetInactivityConfirmationModalWithStartedCountdown(InactivityConfirmationModalModel inactivityConfirmationModalModel)
        {
            var inactivityConfirmationModal = new InactivityConfirmationModal(inactivityConfirmationModalModel);
            inactivityConfirmationModal.StartCountdown();
            return inactivityConfirmationModal;
        }
    }
}