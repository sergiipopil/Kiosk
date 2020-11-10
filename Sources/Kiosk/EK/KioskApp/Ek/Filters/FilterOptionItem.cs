using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Ek.Filters
{
    public class FilterOptionItem : UiBindableObject
    {
        #region Name

        private string _Name;

        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

        #endregion

        public object Value { get; set; }

        public string IconGlyph { get; set; }

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