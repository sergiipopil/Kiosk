using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskBrains.Common.EK.Api;
using KioskBrains.Kiosk.Core.Modals;

namespace KioskApp.Ek.Products.Photos
{
    public sealed partial class ProductPhotosPresenter : UserControl
    {
        public ProductPhotosPresenter()
        {
            InitializeComponent();

            OnPhotosChanged();
        }

        #region Photos

        public static readonly DependencyProperty PhotosProperty = DependencyProperty.Register(
            nameof(Photos), typeof(EkProductPhoto[]), typeof(ProductPhotosPresenter), new PropertyMetadata(default(EkProductPhoto[]), PhotosPropertyChangedCallback));

        private static void PhotosPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProductPhotosPresenter)d).OnPhotosChanged();
        }

        public EkProductPhoto[] Photos
        {
            get => (EkProductPhoto[])GetValue(PhotosProperty);
            set => SetValue(PhotosProperty, value);
        }

        #endregion

        #region PhotoCount

        public static readonly DependencyProperty PhotoCountProperty = DependencyProperty.Register(
            nameof(PhotoCount), typeof(int), typeof(ProductPhotosPresenter), new PropertyMetadata(default(int)));

        public int PhotoCount
        {
            get => (int)GetValue(PhotoCountProperty);
            set => SetValue(PhotoCountProperty, value);
        }

        #endregion

        private void OnPhotosChanged()
        {
            if (Photos?.Length > 0)
            {
                RootElement.Visibility = Visibility.Visible;
                PhotoCount = Photos.Length;
            }
            else
            {
                RootElement.Visibility = Visibility.Collapsed;
            }
        }

        private void EnlargeButton_OnClick(object sender, RoutedEventArgs e)
        {
            ShowProductPhotosPopupAsync(Photos);
        }

        public static Task ShowProductPhotosPopupAsync(EkProductPhoto[] photos)
        {
            return ModalManager.Current.ShowModalAsync(new ModalArgs(
                "ProductPhotos",
                context => new ProductPhotosModal(context)
                    {
                        Photos = photos,
                    },
                showCancelButton: false));
        }
    }
}