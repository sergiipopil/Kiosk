using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Ek.Checkout.Steps
{
    public class PhoneNumberStepData : UiBindableObject
    {
        #region PhoneNumber

        private string _PhoneNumber;

        public string PhoneNumber
        {
            get => _PhoneNumber;
            set => SetProperty(ref _PhoneNumber, value);
        }

        #endregion

        public string ConfirmedPhoneNumber { get; set; }

        #region VerificationCode

        private string _VerificationCode;

        public string VerificationCode
        {
            get => _VerificationCode;
            set => SetProperty(ref _VerificationCode, value);
        }

        #endregion
    }
}