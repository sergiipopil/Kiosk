using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Ek.Checkout.Steps.Payment
{
    public class PaymentMethodInfo : UiBindableObject
    {
        public PaymentMethodInfo(PaymentMethodEnum paymentMethod)
        {
            PaymentMethod = paymentMethod;

            string name;
            switch (paymentMethod)
            {
                case PaymentMethodEnum.CreditCard:
                    name = "Картою\nVisa/MasterCard";
                    break;

                case PaymentMethodEnum.Privat24:
                    name = "Приват24";
                    break;

                case PaymentMethodEnum.CashInTerminal:
                    name = "Готівкою в терміналі\nПриватБанка";
                    break;

                case PaymentMethodEnum.CashInBank:
                    name = "Готівкою в банку";
                    break;

                case PaymentMethodEnum.WireTransfer:
                    name = "Безготівковий платіж\n(юр. особи)";
                    break;

                default:
                    name = "?";
                    break;
            }

            _Name = name;
        }

        public PaymentMethodEnum PaymentMethod { get; }

        #region Name

        private string _Name;

        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

        #endregion

        #region IsSelected

        private bool _IsSelected;

        public bool IsSelected
        {
            get => _IsSelected;
            set => SetProperty(ref _IsSelected, value);
        }

        #endregion
    }
}