using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using KioskApp.Ek.Products.Photos;
using KioskApp.Search;
using KioskBrains.Kiosk.Core.Modals;

namespace KioskApp.Ek.Products
{
    public sealed partial class ResultProductPresenter : UserControl
    {
        public ResultProductPresenter()
        {
            InitializeComponent();
        }

        public EkContext EkContext => EkContext.Current;

        #region Product

        public static readonly DependencyProperty ProductProperty = DependencyProperty.Register(
            nameof(Product), typeof(Product), typeof(ResultProductPresenter), new PropertyMetadata(default(Product)));

        public Product Product
        {
            get => (Product)GetValue(ProductProperty);
            set => SetValue(ProductProperty, value);
        }

        #endregion

        private void Photo_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (Product?.IsNotAvailable != false)
            {
                return;
            }

            var photos = Product.Photos;
            if (photos?.Length > 0)
            {
                ProductPhotosPresenter.ShowProductPhotosPopupAsync(photos);
            }
        }

        private void InfoContainer_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (Product?.IsNotAvailable != false)
            {
                return;
            }

            if (Product.IsDescriptionRequestRequired)
            {
                Product.RequestDescription();
            }

#pragma warning disable 4014
            ModalManager.Current.ShowModalAsync(new ModalArgs(
#pragma warning restore 4014
                "ProductDetails",
                context => new ProductDetailsModal(context)
                    {
                        Product = Product,
                    },
                showCancelButton: false));
        }

        private void BuyButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (Product.IsDescriptionRequestRequired)
            {
                Product.RequestDescription();
            }

            EkContext.EkProcess?.Cart.AddToCartCommand.Execute(Product);
            EkContext.GoToCartCommand?.Execute(null);
        }
    }
}