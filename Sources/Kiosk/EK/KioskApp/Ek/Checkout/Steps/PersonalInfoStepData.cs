using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Ek.Checkout.Steps
{
    public class PersonalInfoStepData : UiBindableObject
    {
        #region FullName

        private string _FullName;

        public string FullName
        {
            get => _FullName;
            set => SetProperty(ref _FullName, value);
        }

        #endregion
    }
}