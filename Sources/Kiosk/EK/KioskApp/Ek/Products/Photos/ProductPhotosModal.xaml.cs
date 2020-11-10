using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.EK.Api;
using KioskBrains.Kiosk.Core.Modals;

namespace KioskApp.Ek.Products.Photos
{
    public sealed partial class ProductPhotosModal : UserControl
    {
        private readonly ModalContext _modalContext;

        public ProductPhotosModal(ModalContext modalContext)
        {
            Assure.ArgumentNotNull(modalContext, nameof(modalContext));

            _modalContext = modalContext;

            InitializeComponent();
        }

        #region Photos

        public static readonly DependencyProperty PhotosProperty = DependencyProperty.Register(
            nameof(Photos), typeof(EkProductPhoto[]), typeof(ProductPhotosModal), new PropertyMetadata(default(EkProductPhoto[]), PhotosPropertyChangedCallback));

        private static void PhotosPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProductPhotosModal)d).OnPhotosChanged();
        }

        public EkProductPhoto[] Photos
        {
            get => (EkProductPhoto[])GetValue(PhotosProperty);
            set => SetValue(PhotosProperty, value);
        }

        #endregion

        #region PhotoItems

        public static readonly DependencyProperty PhotoItemsProperty = DependencyProperty.Register(
            nameof(PhotoItems), typeof(PhotoItem[]), typeof(ProductPhotosModal), new PropertyMetadata(default(PhotoItem[])));

        public PhotoItem[] PhotoItems
        {
            get => (PhotoItem[])GetValue(PhotoItemsProperty);
            set => SetValue(PhotoItemsProperty, value);
        }

        #endregion

        #region SelectedPhotoItem

        public static readonly DependencyProperty SelectedPhotoItemProperty = DependencyProperty.Register(
            nameof(SelectedPhotoItem), typeof(PhotoItem), typeof(ProductPhotosModal), new PropertyMetadata(default(PhotoItem), SelectedPhotoItemPropertyChangedCallback));

        private static void SelectedPhotoItemPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProductPhotosModal)d).OnSelectedPhotoItemChanged();
        }

        public PhotoItem SelectedPhotoItem
        {
            get => (PhotoItem)GetValue(SelectedPhotoItemProperty);
            set => SetValue(SelectedPhotoItemProperty, value);
        }

        #endregion

        private void OnPhotosChanged()
        {
            PhotoItems = Photos
                ?.Select(x => new PhotoItem(x))
                .ToArray();

            if (PhotoItems != null)
            {
                SelectedPhotoItem = PhotoItems.FirstOrDefault();
            }
        }

        private void OnSelectedPhotoItemChanged()
        {
            if (PhotoItems != null)
            {
                foreach (var photoItem in PhotoItems)
                {
                    photoItem.IsSelected = photoItem == SelectedPhotoItem;
                }
            }
        }

        private void ThumbnailButton_OnClick(object sender, RoutedEventArgs e)
        {
            var photoItem = ((Button)sender).DataContext as PhotoItem;
            if (photoItem == null)
            {
                return;
            }

            SelectedPhotoItem = photoItem;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _modalContext.CloseModalAsync();
        }
    }
}