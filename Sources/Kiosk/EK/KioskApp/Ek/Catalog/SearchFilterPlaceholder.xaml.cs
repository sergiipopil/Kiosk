using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KioskApp.Ek.Catalog
{
    public sealed partial class SearchFilterPlaceholder : UserControl
    {
        public SearchFilterPlaceholder()
        {
            InitializeComponent();
        }

        #region PlaceholderText1

        public static readonly DependencyProperty PlaceholderText1Property = DependencyProperty.Register(
            nameof(PlaceholderText1), typeof(string), typeof(SearchFilterPlaceholder), new PropertyMetadata(default(string)));

        public string PlaceholderText1
        {
            get => (string)GetValue(PlaceholderText1Property);
            set => SetValue(PlaceholderText1Property, value);
        }

        #endregion

        #region PlaceholderText2

        public static readonly DependencyProperty PlaceholderText2Property = DependencyProperty.Register(
            nameof(PlaceholderText2), typeof(string), typeof(SearchFilterPlaceholder), new PropertyMetadata(default(string)));

        public string PlaceholderText2
        {
            get => (string)GetValue(PlaceholderText2Property);
            set => SetValue(PlaceholderText2Property, value);
        }

        #endregion
    }
}