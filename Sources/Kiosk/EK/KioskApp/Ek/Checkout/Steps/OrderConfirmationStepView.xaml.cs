using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskApp.Ek.Info;

namespace KioskApp.Ek.Checkout.Steps
{
    public sealed partial class OrderConfirmationStepView : UserControl
    {
        public OrderConfirmationStepView()
        {
            InitializeComponent();
        }

        private void ConfirmButton_OnClick(object sender, RoutedEventArgs e)
        {
            WizardNextCommand?.Execute(null);
        }

        #region WizardBackCommand

        public static readonly DependencyProperty WizardBackCommandProperty = DependencyProperty.Register(
            nameof(WizardBackCommand), typeof(ICommand), typeof(OrderConfirmationStepView), new PropertyMetadata(default(ICommand)));

        public ICommand WizardBackCommand
        {
            get => (ICommand)GetValue(WizardBackCommandProperty);
            set => SetValue(WizardBackCommandProperty, value);
        }

        #endregion

        #region WizardNextCommand

        public static readonly DependencyProperty WizardNextCommandProperty = DependencyProperty.Register(
            nameof(WizardNextCommand), typeof(ICommand), typeof(OrderConfirmationStepView), new PropertyMetadata(default(ICommand)));

        public ICommand WizardNextCommand
        {
            get => (ICommand)GetValue(WizardNextCommandProperty);
            set => SetValue(WizardNextCommandProperty, value);
        }

        #endregion

        private void TermsButton_OnClick(object sender, RoutedEventArgs e)
        {
            InfoModalHelper.OpenInfoModal(InfoModalTypeEnum.UserAgreement);
        }
    }
}