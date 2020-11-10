using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Ek.Checkout.Steps.Payment
{
    public sealed partial class PaymentInfoStepView : UserControl
    {
        public PaymentInfoStepView()
        {
            ConfirmCommand = new RelayCommand(
                nameof(ConfirmCommand),
                parameter =>
                    {
                        if (Data.PaymentMethodInfo == null)
                        {
                            return;
                        }

                        WizardNextCommand?.Execute(null);
                    });

            InitializeComponent();
        }

        public ICommand ConfirmCommand { get; }

        #region Data

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            nameof(Data), typeof(PaymentInfoStepData), typeof(PaymentInfoStepView), new PropertyMetadata(default(PaymentInfoStepData)));

        public PaymentInfoStepData Data
        {
            get => (PaymentInfoStepData)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        #endregion

        #region WizardBackCommand

        public static readonly DependencyProperty WizardBackCommandProperty = DependencyProperty.Register(
            nameof(WizardBackCommand), typeof(ICommand), typeof(PaymentInfoStepView), new PropertyMetadata(default(ICommand)));

        public ICommand WizardBackCommand
        {
            get => (ICommand)GetValue(WizardBackCommandProperty);
            set => SetValue(WizardBackCommandProperty, value);
        }

        #endregion

        #region WizardNextCommand

        public static readonly DependencyProperty WizardNextCommandProperty = DependencyProperty.Register(
            nameof(WizardNextCommand), typeof(ICommand), typeof(PaymentInfoStepView), new PropertyMetadata(default(ICommand)));

        public ICommand WizardNextCommand
        {
            get => (ICommand)GetValue(WizardNextCommandProperty);
            set => SetValue(WizardNextCommandProperty, value);
        }

        #endregion
    }
}