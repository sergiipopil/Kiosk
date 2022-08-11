using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskApp.Search;
using KioskBrains.Common.EK.Api;

namespace KioskApp.Ek.Filters
{
    public sealed partial class ProductSortingSelector : UserControl
    {
        public ProductSortingSelector()
        {
            InitializeComponent();
        }

        public FilterOptionItem[] FilterOptions { get; } =
            {
                new FilterOptionItem()
                    {
                        Name = "звичайне",
                        Value = EkProductSearchSortingEnum.Default,
                    },
                new FilterOptionItem()
                    {
                        Name = "ціна",
                        Value = EkProductSearchSortingEnum.PriceAscending,
                        IconGlyph = "\xF0AD",
                    },
                new FilterOptionItem()
                    {
                        Name = "ціна",
                        Value = EkProductSearchSortingEnum.PriceDescending,
                        IconGlyph = "\xF0AE",
                    },
            };

        #region SelectedValue

        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register(
            nameof(SelectedValue), typeof(EkProductSearchSortingEnum), typeof(ProductSortingSelector), new PropertyMetadata(default(EkProductSearchSortingEnum), SelectedValuePropertyChangedCallback));

        private static void SelectedValuePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProductSortingSelector)d).OnSelectedValueChanged();
        }

        public EkProductSearchSortingEnum SelectedValue
        {
            get => (EkProductSearchSortingEnum)GetValue(SelectedValueProperty);
            set => SetValue(SelectedValueProperty, value);
        }

        #endregion

        private void OnSelectedValueChanged()
        {
            UpdateSelectedFilterOption();
        }

        private void UpdateSelectedFilterOption()
        {
            var selectedValue = SelectedValue;
            foreach (var filterOptionItem in FilterOptions)
            {
                filterOptionItem.IsSelected = (EkProductSearchSortingEnum)filterOptionItem.Value == selectedValue;
            }
        }

        private void FilterOption_OnClick(object sender, RoutedEventArgs e)
        {
            var filterOption = ((FrameworkElement)sender).DataContext as FilterOptionItem;
            if (filterOption == null)
            {
                return;
            }

            SelectedValue = (EkProductSearchSortingEnum)filterOption.Value;
        }
    }
}