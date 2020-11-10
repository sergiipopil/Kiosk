using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using KioskApp.Ek.Products.Photos;
using KioskApp.Search;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;
using KioskBrains.Kiosk.Core.Modals;

namespace KioskApp.Ek.Products
{
    public sealed partial class ProductDetailsModal : UserControl
    {
        private readonly ModalContext _modalContext;

        public ProductDetailsModal(ModalContext modalContext)
        {
            Assure.ArgumentNotNull(modalContext, nameof(modalContext));

            _modalContext = modalContext;

            InitializeComponent();
        }

        public EkContext EkContext => EkContext.Current;

        #region Product

        public static readonly DependencyProperty ProductProperty = DependencyProperty.Register(
            nameof(Product), typeof(Product), typeof(ProductDetailsModal), new PropertyMetadata(default(Product), ProductPropertyChangedCallback));

        private static void ProductPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProductDetailsModal)d).OnProductChanged();
        }

        public Product Product
        {
            get => (Product)GetValue(ProductProperty);
            set => SetValue(ProductProperty, value);
        }

        #endregion

        #region AdditionalPhotos

        public static readonly DependencyProperty AdditionalPhotosProperty = DependencyProperty.Register(
            nameof(AdditionalPhotos), typeof(EkProductPhoto[]), typeof(ProductDetailsModal), new PropertyMetadata(default(EkProductPhoto[])));

        public EkProductPhoto[] AdditionalPhotos
        {
            get => (EkProductPhoto[])GetValue(AdditionalPhotosProperty);
            set => SetValue(AdditionalPhotosProperty, value);
        }

        #endregion

        private void OnProductChanged()
        {
            var photos = Product?.Photos;
            if (photos?.Length > 1)
            {
                AdditionalPhotos = photos
                    .Skip(1)
                    .ToArray();
            }
            else
            {
                AdditionalPhotos = new EkProductPhoto[0];
            }

            if (Product?.EkProduct.Source != EkProductSourceEnum.AllegroPl)
            {
                RootElement.Height = 600;
            }
            else
            {
                RootElement.Height = 800;
            }
        }

        private void Photos_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var photos = Product?.Photos;
            if (photos?.Length > 0)
            {
                ProductPhotosPresenter.ShowProductPhotosPopupAsync(photos);
            }
        }

        private void BuyButton_OnClick(object sender, RoutedEventArgs e)
        {
            _modalContext.CloseModalAsync();
            EkContext.EkProcess?.Cart.AddToCartCommand.Execute(Product);
            EkContext.GoToCartCommand?.Execute(null);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _modalContext.CloseModalAsync();
        }
    }
}