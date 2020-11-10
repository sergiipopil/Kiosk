using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using KioskBrains.Common.EK.Transactions;

namespace KioskApp.Ek.Checkout.Steps.Delivery
{
    public sealed partial class DeliveryTypeOptionPresenter : UserControl
    {
        public DeliveryTypeOptionPresenter()
        {
            InitializeComponent();
        }

        public EkDeliveryTypeEnum DeliveryType { get; set; }

        #region Icon

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon), typeof(ImageSource), typeof(DeliveryTypeOptionPresenter), new PropertyMetadata(default(ImageSource)));

        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        #endregion

        #region Text1

        public static readonly DependencyProperty Text1Property = DependencyProperty.Register(
            nameof(Text1), typeof(string), typeof(DeliveryTypeOptionPresenter), new PropertyMetadata(default(string)));

        public string Text1
        {
            get => (string)GetValue(Text1Property);
            set => SetValue(Text1Property, value);
        }

        #endregion

        #region Text2

        public static readonly DependencyProperty Text2Property = DependencyProperty.Register(
            nameof(Text2), typeof(string), typeof(DeliveryTypeOptionPresenter), new PropertyMetadata(default(string)));

        public string Text2
        {
            get => (string)GetValue(Text2Property);
            set => SetValue(Text2Property, value);
        }

        #endregion

        private void Container_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            OnSelected(DeliveryType);
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            OnSelected(DeliveryType);
        }

        public event EventHandler<EkDeliveryTypeEnum> Selected;

        private void OnSelected(EkDeliveryTypeEnum e)
        {
            Selected?.Invoke(this, e);
        }
    }
}