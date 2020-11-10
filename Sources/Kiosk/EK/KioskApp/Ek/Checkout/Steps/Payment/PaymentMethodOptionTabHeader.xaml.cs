using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace KioskApp.Ek.Checkout.Steps.Payment
{
    public sealed partial class PaymentMethodOptionTabHeader : UserControl
    {
        public PaymentMethodOptionTabHeader()
        {
            InitializeComponent();
        }

        #region PaymentMethod

        public static readonly DependencyProperty PaymentMethodProperty = DependencyProperty.Register(
            nameof(PaymentMethod), typeof(PaymentMethodInfo), typeof(PaymentMethodOptionTabHeader), new PropertyMetadata(default(PaymentMethodInfo), PaymentMethodPropertyChangedCallback));

        private static void PaymentMethodPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PaymentMethodOptionTabHeader)d).OnPaymentMethodChanged();
        }

        public PaymentMethodInfo PaymentMethod
        {
            get => (PaymentMethodInfo)GetValue(PaymentMethodProperty);
            set => SetValue(PaymentMethodProperty, value);
        }

        #endregion

        private void OnPaymentMethodChanged()
        {
            if (PaymentMethod == null)
            {
                Icon = null;
            }
            else
            {
                Icon = PaymentMethodIcons.GetIcon(PaymentMethod.PaymentMethod, false);
            }
        }

        #region Icon

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon), typeof(ImageSource), typeof(PaymentMethodOptionTabHeader), new PropertyMetadata(default(ImageSource)));

        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        #endregion

        private void Container_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            OnPressed();
        }

        public event EventHandler<PaymentMethodInfo> Pressed;

        private void OnPressed()
        {
            Pressed?.Invoke(this, PaymentMethod);
        }
    }
}