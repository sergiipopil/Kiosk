using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Ek.Checkout.Steps.Delivery
{
    public class DeliveryInfoAddress : UiBindableObject
    {
        #region City

        private string _City;

        public string City
        {
            get => _City;
            set => SetProperty(ref _City, value);
        }

        #endregion

        #region AddressLine1

        private string _AddressLine1;

        public string AddressLine1
        {
            get => _AddressLine1;
            set => SetProperty(ref _AddressLine1, value);
        }

        #endregion
    }
}