using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskApp.Search;

namespace KioskApp.Ek.Catalog.AutoParts.Europe
{
    public sealed partial class EuropeSearchRightView : UserControl
    {
        public EuropeSearchRightView()
        {
            InitializeComponent();
        }

        #region SearchProvider

        public static readonly DependencyProperty SearchProviderProperty = DependencyProperty.Register(
            nameof(SearchProvider), typeof(ProductSearchInEuropeProvider), typeof(EuropeSearchRightView), new PropertyMetadata(default(ProductSearchInEuropeProvider)));

        public ProductSearchInEuropeProvider SearchProvider
        {
            get => (ProductSearchInEuropeProvider)GetValue(SearchProviderProperty);
            set => SetValue(SearchProviderProperty, value);
        }

        #endregion

        #region BackCommand

        public static readonly DependencyProperty BackCommandProperty = DependencyProperty.Register(
            nameof(BackCommand), typeof(ICommand), typeof(EuropeSearchRightView), new PropertyMetadata(default(ICommand)));

        public ICommand BackCommand
        {
            get => (ICommand)GetValue(BackCommandProperty);
            set => SetValue(BackCommandProperty, value);
        }

        #endregion
    }
}