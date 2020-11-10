using KioskBrains.Common.EK.Api;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Ek.Products.Photos
{
    public class PhotoItem : UiBindableObject
    {
        public EkProductPhoto Photo { get; }

        public PhotoItem(EkProductPhoto photo)
        {
            Photo = photo;
        }

        #region IsSelected

        private bool _IsSelected;

        public bool IsSelected
        {
            get => _IsSelected;
            set => SetProperty(ref _IsSelected, value);
        }

        #endregion
    }
}