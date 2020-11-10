using KioskBrains.Kiosk.Core.Languages;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Ek.Catalog.AutoParts
{
    public class SearchByAnotherTypeMenuItem : UiBindableObject
    {
        public SearchByAnotherTypeMenuItem(string textResourceKey, SearchTypeEnum searchType)
        {
            TextResourceKey = textResourceKey;
            SearchType = searchType;
        }

        public string TextResourceKey { get; }

        public SearchTypeEnum SearchType { get; }

        public void UpdateText()
        {
            Text = LanguageManager.Current.GetLocalizedString(TextResourceKey);
        }

        #region Text

        private string _Text;

        public string Text
        {
            get => _Text;
            set => SetProperty(ref _Text, value);
        }

        #endregion

        #region IsVisible

        private bool _IsVisible;

        public bool IsVisible
        {
            get => _IsVisible;
            set => base.SetProperty(ref _IsVisible, value);
        }

        #endregion
    }
}