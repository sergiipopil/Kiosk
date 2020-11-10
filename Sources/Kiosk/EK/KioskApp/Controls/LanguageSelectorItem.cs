using Windows.Globalization;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Controls
{
    public class LanguageSelectorItem : UiBindableObject
    {
        public LanguageSelectorItem(Language language)
        {
            Language = language;
        }

        public Language Language { get; }

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