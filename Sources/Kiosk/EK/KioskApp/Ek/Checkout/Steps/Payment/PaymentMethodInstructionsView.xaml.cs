using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskApp.Ek.Checkout.Steps.Payment.Instructions;

namespace KioskApp.Ek.Checkout.Steps.Payment
{
    public sealed partial class PaymentMethodInstructionsView : UserControl
    {
        public PaymentMethodInstructionsView()
        {
            InitializeComponent();

            _allMethods = new[]
                {
                    CreditCardMethod,
                    Privat24Method,
                    CashInTerminalMethod,
                    CashInBankMethod,
                    WireTransferMethod,
                };
        }

        #region SelectedPaymentMethod

        public static readonly DependencyProperty SelectedPaymentMethodProperty = DependencyProperty.Register(
            nameof(SelectedPaymentMethod), typeof(PaymentMethodEnum?), typeof(PaymentMethodInstructionsView), new PropertyMetadata(default(PaymentMethodInfo), SelectedPaymentMethodPropertyChangedCallback));

        private static void SelectedPaymentMethodPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PaymentMethodInstructionsView)d).OnSelectedPaymentMethodChanged();
        }

        public PaymentMethodInfo SelectedPaymentMethod
        {
            get => (PaymentMethodInfo)GetValue(SelectedPaymentMethodProperty);
            set => SetValue(SelectedPaymentMethodProperty, value);
        }

        #endregion

        #region BackCommand

        public static readonly DependencyProperty BackCommandProperty = DependencyProperty.Register(
            nameof(BackCommand), typeof(ICommand), typeof(PaymentMethodInstructionsView), new PropertyMetadata(default(ICommand)));

        public ICommand BackCommand
        {
            get => (ICommand)GetValue(BackCommandProperty);
            set => SetValue(BackCommandProperty, value);
        }

        #endregion

        #region ConfirmCommand

        public static readonly DependencyProperty ConfirmCommandProperty = DependencyProperty.Register(
            nameof(ConfirmCommand), typeof(ICommand), typeof(PaymentMethodInstructionsView), new PropertyMetadata(default(ICommand)));

        public ICommand ConfirmCommand
        {
            get => (ICommand)GetValue(ConfirmCommandProperty);
            set => SetValue(ConfirmCommandProperty, value);
        }

        #endregion

        #region IsConfirmed

        public static readonly DependencyProperty IsConfirmedProperty = DependencyProperty.Register(
            nameof(IsConfirmed), typeof(bool), typeof(PaymentMethodInstructionsView), new PropertyMetadata(default(bool)));

        public bool IsConfirmed
        {
            get => (bool)GetValue(IsConfirmedProperty);
            set => SetValue(IsConfirmedProperty, value);
        }

        #endregion

        #region PaymentMethodInstructionsPresenter

        public static readonly DependencyProperty PaymentMethodInstructionsPresenterProperty = DependencyProperty.Register(
            nameof(PaymentMethodInstructionsPresenter), typeof(UserControl), typeof(PaymentMethodInstructionsView), new PropertyMetadata(default(UserControl)));

        public UserControl PaymentMethodInstructionsPresenter
        {
            get => (UserControl)GetValue(PaymentMethodInstructionsPresenterProperty);
            set => SetValue(PaymentMethodInstructionsPresenterProperty, value);
        }

        #endregion

        private readonly PaymentMethodInfo[] _allMethods;

        public PaymentMethodInfo CreditCardMethod { get; } = new PaymentMethodInfo(PaymentMethodEnum.CreditCard);

        public PaymentMethodInfo Privat24Method { get; } = new PaymentMethodInfo(PaymentMethodEnum.Privat24);

        public PaymentMethodInfo CashInTerminalMethod { get; } = new PaymentMethodInfo(PaymentMethodEnum.CashInTerminal);

        public PaymentMethodInfo CashInBankMethod { get; } = new PaymentMethodInfo(PaymentMethodEnum.CashInBank);

        public PaymentMethodInfo WireTransferMethod { get; } = new PaymentMethodInfo(PaymentMethodEnum.WireTransfer);

        private void OnSelectedPaymentMethodChanged()
        {
            if (SelectedPaymentMethod != null)
            {
                var paymentMethod = SelectedPaymentMethod.PaymentMethod;
                foreach (var paymentMethodInfo in _allMethods)
                {
                    paymentMethodInfo.IsSelected = paymentMethodInfo.PaymentMethod == paymentMethod;
                }

                switch (paymentMethod)
                {
                    case PaymentMethodEnum.CreditCard:
                        PaymentMethodInstructionsPresenter = new CreditCardMethodInstructionsPresenter();
                        break;
                    case PaymentMethodEnum.Privat24:
                        PaymentMethodInstructionsPresenter = new Privat24MethodInstructionsPresenter();
                        break;
                    case PaymentMethodEnum.CashInTerminal:
                        PaymentMethodInstructionsPresenter = new CashInTerminalMethodInstructionsPresenter();
                        break;
                    case PaymentMethodEnum.CashInBank:
                        PaymentMethodInstructionsPresenter = new CashInBankMethodInstructionsPresenter();
                        break;
                    case PaymentMethodEnum.WireTransfer:
                        PaymentMethodInstructionsPresenter = new WireTransferMethodInstructionsPresenter();
                        break;
                    default:
                        PaymentMethodInstructionsPresenter = null;
                        break;
                }
            }
            else
            {
                PaymentMethodInstructionsPresenter = null;
            }
        }

        private void PaymentMethodOptionBigPresenter_OnSelected(object sender, PaymentMethodInfo e)
        {
            var paymentMethod = ((PaymentMethodOptionBigPresenter)sender).PaymentMethod;
            if (paymentMethod == null)
            {
                return;
            }

            SelectedPaymentMethod = paymentMethod;
        }

        private void PaymentMethodOptionTabHeader_OnPressed(object sender, PaymentMethodInfo e)
        {
            var paymentMethod = ((PaymentMethodOptionTabHeader)sender).PaymentMethod;
            if (paymentMethod == null)
            {
                return;
            }

            SelectedPaymentMethod = paymentMethod;
        }

        private void ConfirmButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedPaymentMethod == null)
            {
                return;
            }

            ConfirmCommand?.Execute(SelectedPaymentMethod);
        }
    }
}