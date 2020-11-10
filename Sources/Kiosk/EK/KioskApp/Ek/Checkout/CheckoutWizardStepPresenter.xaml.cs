using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KioskApp.Ek.Checkout
{
    public sealed partial class CheckoutWizardStepPresenter : UserControl
    {
        public CheckoutWizardStepPresenter()
        {
            InitializeComponent();
        }

        #region StepItem

        public static readonly DependencyProperty StepItemProperty = DependencyProperty.Register(
            nameof(StepItem), typeof(CheckoutWizardStep), typeof(CheckoutWizardStepPresenter), new PropertyMetadata(default(CheckoutWizardStep)));

        public CheckoutWizardStep StepItem
        {
            get => (CheckoutWizardStep)GetValue(StepItemProperty);
            set => SetValue(StepItemProperty, value);
        }

        #endregion
    }
}