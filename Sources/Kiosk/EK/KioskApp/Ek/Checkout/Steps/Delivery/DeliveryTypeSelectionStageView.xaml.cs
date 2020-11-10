using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskBrains.Common.EK.Transactions;

namespace KioskApp.Ek.Checkout.Steps.Delivery
{
    public sealed partial class DeliveryTypeSelectionStageView : UserControl
    {
        public DeliveryTypeSelectionStageView()
        {
            InitializeComponent();
        }

        #region BackCommand

        public static readonly DependencyProperty BackCommandProperty = DependencyProperty.Register(
            nameof(BackCommand), typeof(ICommand), typeof(DeliveryTypeSelectionStageView), new PropertyMetadata(default(ICommand)));

        public ICommand BackCommand
        {
            get => (ICommand)GetValue(BackCommandProperty);
            set => SetValue(BackCommandProperty, value);
        }

        #endregion

        private void DeliveryTypeOptionPresenter_OnSelected(object sender, EkDeliveryTypeEnum e)
        {
            OnDeliveryTypeSelected(e);
        }

        public event EventHandler<EkDeliveryTypeEnum> DeliveryTypeSelected;

        private void OnDeliveryTypeSelected(EkDeliveryTypeEnum e)
        {
            DeliveryTypeSelected?.Invoke(this, e);
        }
    }
}