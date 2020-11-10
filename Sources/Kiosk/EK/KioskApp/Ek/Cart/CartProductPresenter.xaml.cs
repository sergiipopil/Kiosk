using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskApp.Search;

namespace KioskApp.Ek.Cart
{
    public sealed partial class CartProductPresenter : UserControl
    {
        public CartProductPresenter()
        {
            InitializeComponent();
        }

        #region CartProduct

        public static readonly DependencyProperty CartProductProperty = DependencyProperty.Register(
            nameof(CartProduct), typeof(CartProduct), typeof(CartProductPresenter), new PropertyMetadata(default(CartProduct), CartProductPropertyChangedCallback));

        private static void CartProductPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CartProductPresenter)d).OnCartProductChanged();
        }

        public CartProduct CartProduct
        {
            get => (CartProduct)GetValue(CartProductProperty);
            set => SetValue(CartProductProperty, value);
        }

        #endregion

        private void OnCartProductChanged()
        {
            Product = CartProduct?.Product;
        }

        #region Product

        public static readonly DependencyProperty ProductProperty = DependencyProperty.Register(
            nameof(Product), typeof(Product), typeof(CartProductPresenter), new PropertyMetadata(default(Product)));

        public Product Product
        {
            get => (Product)GetValue(ProductProperty);
            set => SetValue(ProductProperty, value);
        }

        #endregion

        private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (CartProduct == null)
            {
                return;
            }

            CartProduct.Quantity = 0;
        }

        private void QuantityDecrementButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (CartProduct == null)
            {
                return;
            }

            CartProduct.Quantity--;
        }

        private void QuantityIncrementButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (CartProduct == null)
            {
                return;
            }

            CartProduct.Quantity++;
        }
    }
}