using KioskApp.Search;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Ek.Catalog.AutoParts
{
    public class SearchByCategoryContext : UiBindableObject
    {
        #region SelectedModification

        private SelectedCategoryValue _SelectedModification;

        public SelectedCategoryValue SelectedModification
        {
            get => _SelectedModification;
            set => SetProperty(ref _SelectedModification, value);
        }

        #endregion

        #region SelectedCategory

        private SelectedCategoryValue _SelectedCategory;

        public SelectedCategoryValue SelectedCategory
        {
            get => _SelectedCategory;
            set => SetProperty(ref _SelectedCategory, value);
        }

        #endregion
    }
}