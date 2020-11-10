using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskBrains.Kiosk.Core.Languages;

namespace KioskApp.Controls
{
    public sealed partial class LanguageSelector : UserControl
    {
        public LanguageSelector()
        {
            Items = LanguageManager.Current.KioskLanguages
                .Select(x => new LanguageSelectorItem(x))
                .ToArray();
            SetSelectedItemBySelectedLanguage();

            InitializeComponent();
        }

        public LanguageSelectorItem[] Items { get; set; }

        private void LanguageSelector_OnLoaded(object sender, RoutedEventArgs e)
        {
            LanguageManager.Current.LanguageChanged += OnLanguageChanged;
        }

        private void LanguageSelector_OnUnloaded(object sender, RoutedEventArgs e)
        {
            LanguageManager.Current.LanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged(object sender, Windows.Globalization.Language e)
        {
            SetSelectedItemBySelectedLanguage();
        }

        private void SetSelectedItemBySelectedLanguage()
        {
            var selectedLanguage = LanguageManager.Current.CurrentAppLanguage;
            foreach (var item in Items)
            {
                item.IsSelected = item.Language == selectedLanguage;
            }
        }

        private void LanguageSelectorItem_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = (LanguageSelectorItem)((Button)sender).DataContext;
            if (selectedItem.IsSelected)
            {
                return;
            }

            OnLanguageSelected(new LanguageSelectedEventArgs(selectedItem.Language));
        }

        public event EventHandler<LanguageSelectedEventArgs> LanguageSelected;

        private void OnLanguageSelected(LanguageSelectedEventArgs e)
        {
            LanguageSelected?.Invoke(this, e);
        }
    }
}