using Windows.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskApp.Controls.Keyboards;
using KioskApp.Search;
using KioskBrains.Kiosk.Core.Languages;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskApp.Ek.Catalog.AutoParts.Europe
{
    public sealed partial class EuropeSearchLeftView : UserControl
    {
        public EuropeSearchLeftView(bool isCategorySelected)
        {
            IsCategorySelected = isCategorySelected;

            if (!isCategorySelected)
            {
                KeyboardLayoutProvider = new SearchByPartNumberKeyboardLayoutProvider();
            }
            

            InitializeComponent();
            if (isCategorySelected)
            {
                searchAndKeybord.Visibility = Visibility.Collapsed;
                return;
            }
        }

        #region SearchProvider

        public static readonly DependencyProperty SearchProviderProperty = DependencyProperty.Register(
            nameof(SearchProvider), typeof(SearchProviderBase), typeof(EuropeSearchLeftView), new PropertyMetadata(default(SearchProviderBase)));

        public SearchProviderBase SearchProvider
        {
            get => (SearchProviderBase)GetValue(SearchProviderProperty);
            set => SetValue(SearchProviderProperty, value);
        }

        #endregion

        #region SearchInputInstruction

        public static readonly DependencyProperty SearchInputInstructionProperty = DependencyProperty.Register(
            nameof(SearchInputInstruction), typeof(string), typeof(EuropeSearchLeftView), new PropertyMetadata(default(string)));

        public string SearchInputInstruction
        {
            get => (string)GetValue(SearchInputInstructionProperty);
            set => SetValue(SearchInputInstructionProperty, value);
        }

        #endregion

        // as fast workaround to affect the instruction
        public bool IsCategorySelected { get; }

        public IVirtualKeyboardLayoutProvider KeyboardLayoutProvider { get; }

        private void EuropeSearchLeftView_OnLoaded(object sender, RoutedEventArgs e)
        {
            EkContext.Current.HideMenuCounter.Increment();

            LanguageManager.Current.LanguageChanged += OnLanguageChanged;

            UpdateLabels();
        }

        private void EuropeSearchLeftView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            EkContext.Current.HideMenuCounter.Decrement();

            LanguageManager.Current.LanguageChanged -= OnLanguageChanged;
        }

        #region Globalization

        private void UpdateLabels()
        {
            ThreadHelper.EnsureUiThread();
            //fix Sergii
            SearchInputInstruction = "Введіть номер деталі";
        }

        private void OnLanguageChanged(object sender, Language e)
        {
            ThreadHelper.RunInUiThreadAsync(UpdateLabels);
        }

        #endregion
    }
}