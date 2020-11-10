using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskApp.Search;

namespace KioskApp.Ek.Checkout.Steps.Delivery
{
    public sealed partial class NovaPoshtaUkraineBranchSelectionStageView : UserControl
    {
        public NovaPoshtaUkraineBranchSelectionStageView()
        {
            SearchProvider = new NovaPoshtaUkraineBranchSearchProvider();

            InitializeComponent();
        }

        #region BackCommand

        public static readonly DependencyProperty BackCommandProperty = DependencyProperty.Register(
            nameof(BackCommand), typeof(ICommand), typeof(NovaPoshtaUkraineBranchSelectionStageView), new PropertyMetadata(default(ICommand)));

        public ICommand BackCommand
        {
            get => (ICommand)GetValue(BackCommandProperty);
            set => SetValue(BackCommandProperty, value);
        }

        #endregion

        public NovaPoshtaUkraineBranchSearchProvider SearchProvider { get; }

        private void BranchButton_OnClick(object sender, RoutedEventArgs e)
        {
            var branch = ((Button)sender).DataContext as NovaPoshtaUkraineBranch;
            if (branch == null)
            {
                return;
            }

            OnBranchSelected(branch);
        }

        public event EventHandler<NovaPoshtaUkraineBranch> BranchSelected;

        private void OnBranchSelected(NovaPoshtaUkraineBranch e)
        {
            BranchSelected?.Invoke(this, e);
        }
    }
}