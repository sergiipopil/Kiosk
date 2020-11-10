using KioskApp.Controls;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Ek.Checkout
{
    public class CheckoutWizardStep : UiBindableObject
    {
        public CheckoutWizardStep(CheckoutStepEnum step)
        {
            Step = step;
            StepNumber = (int)step;

            UpdateState();
        }

        public CheckoutStepEnum Step { get; }

        public int StepNumber { get; }

        #region Name

        private string _Name;

        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

        #endregion

        #region Value

        private string _Value;

        public string Value
        {
            get => _Value;
            set => SetProperty(ref _Value, value);
        }

        #endregion

        #region IsActive

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => SetProperty(ref _IsActive, value);
        }

        #endregion

        #region State

        private WizardStepStateEnum _State;

        public WizardStepStateEnum State
        {
            get => _State;
            set => SetProperty(ref _State, value);
        }

        #endregion

        protected override void OnOwnPropertyChanged(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Value):
                case nameof(IsActive):
                    UpdateState();
                    break;
            }
        }

        private void UpdateState()
        {
            if (IsActive)
            {
                State = WizardStepStateEnum.Active;
                return;
            }

            State = Value == null
                ? WizardStepStateEnum.Next
                : WizardStepStateEnum.Completed;
        }
    }
}