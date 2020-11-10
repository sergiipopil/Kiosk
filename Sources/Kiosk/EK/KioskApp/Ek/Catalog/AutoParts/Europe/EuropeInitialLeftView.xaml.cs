using System.Windows.Input;
using Windows.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using KioskBrains.Kiosk.Core.Languages;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskApp.Ek.Catalog.AutoParts.Europe
{
    public sealed partial class EuropeInitialLeftView : UserControl
    {
        public EuropeInitialLeftView()
        {
            InitializeComponent();
        }

        private void EuropeInitialLeftView_OnLoaded(object sender, RoutedEventArgs e)
        {
            LanguageManager.Current.LanguageChanged += OnLanguageChanged;

            UpdateLabels();
        }

        private void EuropeInitialLeftView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            LanguageManager.Current.LanguageChanged -= OnLanguageChanged;
        }

        #region EuropeInitialLeftView_FindBy_Line1

        public static readonly DependencyProperty EuropeInitialLeftView_FindBy_Line1Property = DependencyProperty.Register(
            nameof(EuropeInitialLeftView_FindBy_Line1), typeof(string), typeof(EuropeInitialLeftView), new PropertyMetadata(default(string)));

        public string EuropeInitialLeftView_FindBy_Line1
        {
            get => (string)GetValue(EuropeInitialLeftView_FindBy_Line1Property);
            set => SetValue(EuropeInitialLeftView_FindBy_Line1Property, value);
        }

        #endregion

        #region EuropeInitialLeftView_FindBy_Line2_1

        public static readonly DependencyProperty EuropeInitialLeftView_FindBy_Line2_1Property = DependencyProperty.Register(
            nameof(EuropeInitialLeftView_FindBy_Line2_1), typeof(string), typeof(EuropeInitialLeftView), new PropertyMetadata(default(string)));

        public string EuropeInitialLeftView_FindBy_Line2_1
        {
            get => (string)GetValue(EuropeInitialLeftView_FindBy_Line2_1Property);
            set => SetValue(EuropeInitialLeftView_FindBy_Line2_1Property, value);
        }

        #endregion

        #region EuropeInitialLeftView_FindBy_Line2_2

        public static readonly DependencyProperty EuropeInitialLeftView_FindBy_Line2_2Property = DependencyProperty.Register(
            nameof(EuropeInitialLeftView_FindBy_Line2_2), typeof(string), typeof(EuropeInitialLeftView), new PropertyMetadata(default(string)));

        public string EuropeInitialLeftView_FindBy_Line2_2
        {
            get => (string)GetValue(EuropeInitialLeftView_FindBy_Line2_2Property);
            set => SetValue(EuropeInitialLeftView_FindBy_Line2_2Property, value);
        }

        #endregion

        #region EuropeInitialLeftView_FindBy_Line3_1

        public static readonly DependencyProperty EuropeInitialLeftView_FindBy_Line3_1Property = DependencyProperty.Register(
            nameof(EuropeInitialLeftView_FindBy_Line3_1), typeof(string), typeof(EuropeInitialLeftView), new PropertyMetadata(default(string)));

        public string EuropeInitialLeftView_FindBy_Line3_1
        {
            get => (string)GetValue(EuropeInitialLeftView_FindBy_Line3_1Property);
            set => SetValue(EuropeInitialLeftView_FindBy_Line3_1Property, value);
        }

        #endregion

        #region EuropeInitialLeftView_FindBy_Line3_2

        public static readonly DependencyProperty EuropeInitialLeftView_FindBy_Line3_2Property = DependencyProperty.Register(
            nameof(EuropeInitialLeftView_FindBy_Line3_2), typeof(string), typeof(EuropeInitialLeftView), new PropertyMetadata(default(string)));

        public string EuropeInitialLeftView_FindBy_Line3_2
        {
            get => (string)GetValue(EuropeInitialLeftView_FindBy_Line3_2Property);
            set => SetValue(EuropeInitialLeftView_FindBy_Line3_2Property, value);
        }

        #endregion

        #region SearchFilterPlaceholder_FindBy_NameOrCode1

        public static readonly DependencyProperty SearchFilterPlaceholder_FindBy_NameOrCode1Property = DependencyProperty.Register(
            nameof(SearchFilterPlaceholder_FindBy_NameOrCode1), typeof(string), typeof(EuropeInitialLeftView), new PropertyMetadata(default(string)));

        public string SearchFilterPlaceholder_FindBy_NameOrCode1
        {
            get => (string)GetValue(SearchFilterPlaceholder_FindBy_NameOrCode1Property);
            set => SetValue(SearchFilterPlaceholder_FindBy_NameOrCode1Property, value);
        }

        #endregion

        #region SearchFilterPlaceholder_FindBy_NameOrCode2

        public static readonly DependencyProperty SearchFilterPlaceholder_FindBy_NameOrCode2Property = DependencyProperty.Register(
            nameof(SearchFilterPlaceholder_FindBy_NameOrCode2), typeof(string), typeof(EuropeInitialLeftView), new PropertyMetadata(default(string)));

        public string SearchFilterPlaceholder_FindBy_NameOrCode2
        {
            get => (string)GetValue(SearchFilterPlaceholder_FindBy_NameOrCode2Property);
            set => SetValue(SearchFilterPlaceholder_FindBy_NameOrCode2Property, value);
        }

        #endregion

        private void UpdateLabels()
        {
            ThreadHelper.EnsureUiThread();

            EuropeInitialLeftView_FindBy_Line1 = LanguageManager.Current.GetLocalizedString("EuropeInitialLeftView_FindBy_Line1");
            EuropeInitialLeftView_FindBy_Line2_1 = LanguageManager.Current.GetLocalizedString("EuropeInitialLeftView_FindBy_Line2_1");
            EuropeInitialLeftView_FindBy_Line2_2 = LanguageManager.Current.GetLocalizedString("EuropeInitialLeftView_FindBy_Line2_2");
            EuropeInitialLeftView_FindBy_Line3_1 = LanguageManager.Current.GetLocalizedString("EuropeInitialLeftView_FindBy_Line3_1");
            EuropeInitialLeftView_FindBy_Line3_2 = LanguageManager.Current.GetLocalizedString("EuropeInitialLeftView_FindBy_Line3_2");
            SearchFilterPlaceholder_FindBy_NameOrCode1 = LanguageManager.Current.GetLocalizedString("SearchFilterPlaceholder_FindBy_NameOrCode1");
            SearchFilterPlaceholder_FindBy_NameOrCode2 = LanguageManager.Current.GetLocalizedString("SearchFilterPlaceholder_FindBy_NameOrCode2");
        }

        private void OnLanguageChanged(object sender, Language e)
        {
            ThreadHelper.RunInUiThreadAsync(UpdateLabels);
        }

        #region SearchTypeSelectedCommand

        public static readonly DependencyProperty SearchTypeSelectedCommandProperty = DependencyProperty.Register(
            nameof(SearchTypeSelectedCommand), typeof(ICommand), typeof(EuropeInitialLeftView), new PropertyMetadata(default(ICommand)));

        public ICommand SearchTypeSelectedCommand
        {
            get => (ICommand)GetValue(SearchTypeSelectedCommandProperty);
            set => SetValue(SearchTypeSelectedCommandProperty, value);
        }

        #endregion

        private void SearchFilterPlaceholder_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var searchType = (SearchTypeEnum)((SearchFilterPlaceholder)sender).Tag;
            SearchTypeSelectedCommand?.Execute(searchType);
        }
    }
}