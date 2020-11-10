using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskBrains.Common.EK.Api;

namespace KioskApp.Ek.Filters
{
    public sealed partial class ProductStateFilter : UserControl
    {
        public ProductStateFilter()
        {
            FilterOptions = new[]
                {
                    new FilterOptionItem()
                        {
                            Name = "все",
                            Value = null,
                        },
                    new FilterOptionItem()
                        {
                            Name = "новое",
                            Value = EkProductStateEnum.New,
                        },
                    new FilterOptionItem()
                        {
                            Name = "б/у",
                            Value = EkProductStateEnum.Used,
                        },
                    new FilterOptionItem()
                        {
                            Name = "восст.",
                            Value = EkProductStateEnum.Recovered,
                        },
                };

            UpdateSelectedFilterOption();

            InitializeComponent();
        }

        public FilterOptionItem[] FilterOptions { get; }

        #region SelectedValue

        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register(
            nameof(SelectedValue), typeof(EkProductStateEnum?), typeof(ProductStateFilter), new PropertyMetadata(default(EkProductStateEnum?), SelectedValuePropertyChangedCallback));

        private static void SelectedValuePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProductStateFilter)d).OnSelectedValueChanged();
        }

        public EkProductStateEnum? SelectedValue
        {
            get => (EkProductStateEnum?)GetValue(SelectedValueProperty);
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
                filterOptionItem.IsSelected = (EkProductStateEnum?)filterOptionItem.Value == selectedValue;
            }
        }

        private void FilterOption_OnClick(object sender, RoutedEventArgs e)
        {
            var filterOption = ((FrameworkElement)sender).DataContext as FilterOptionItem;
            if (filterOption == null)
            {
                return;
            }

            SelectedValue = (EkProductStateEnum?)filterOption.Value;
        }
    }
}