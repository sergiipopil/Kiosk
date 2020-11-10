using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskApp.Ek.Checkout.Steps.Payment;

namespace KioskApp.Ek.OrderCompletion
{
    public sealed partial class OrderCompletionView : UserControl
    {
        public OrderCompletionView()
        {
            InitializeComponent();
        }

        #region NextUserCommand

        public static readonly DependencyProperty NextUserCommandProperty = DependencyProperty.Register(
            nameof(NextUserCommand), typeof(ICommand), typeof(OrderCompletionView), new PropertyMetadata(default(ICommand)));

        public ICommand NextUserCommand
        {
            get => (ICommand)GetValue(NextUserCommandProperty);
            set => SetValue(NextUserCommandProperty, value);
        }

        #endregion

        #region SelectedPaymentMethodInfo

        public static readonly DependencyProperty SelectedPaymentMethodInfoProperty = DependencyProperty.Register(
            nameof(SelectedPaymentMethodInfo), typeof(PaymentMethodInfo), typeof(OrderCompletionView), new PropertyMetadata(default(PaymentMethodInfo)));

        public PaymentMethodInfo SelectedPaymentMethodInfo
        {
            get => (PaymentMethodInfo)GetValue(SelectedPaymentMethodInfoProperty);
            set => SetValue(SelectedPaymentMethodInfoProperty, value);
        }

        #endregion
    }
}