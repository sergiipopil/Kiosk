using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Ek.Checkout.Steps.Payment
{
    public class PaymentInfoStepData : UiBindableObject
    {
        #region PaymentMethodInfo

        private PaymentMethodInfo _PaymentMethodInfo;

        public PaymentMethodInfo PaymentMethodInfo
        {
            get => _PaymentMethodInfo;
            set => SetProperty(ref _PaymentMethodInfo, value);
        }

        #endregion
    }
}