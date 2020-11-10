using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskApp.Search;

namespace KioskApp.Ek.Products
{
    public sealed partial class ProductResultsView : UserControl
    {
        public ProductResultsView()
        {
            InitializeComponent();
        }

        #region ProductSearchDataSource

        public static readonly DependencyProperty ProductSearchDataSourceProperty = DependencyProperty.Register(
            nameof(ProductSearchDataSource), typeof(IProductSearchDataSource), typeof(ProductResultsView), new PropertyMetadata(default(IProductSearchDataSource)));

        public IProductSearchDataSource ProductSearchDataSource
        {
            get => (IProductSearchDataSource)GetValue(ProductSearchDataSourceProperty);
            set => SetValue(ProductSearchDataSourceProperty, value);
        }

        #endregion
    }
}