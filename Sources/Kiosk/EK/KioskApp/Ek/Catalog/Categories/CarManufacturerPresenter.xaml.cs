using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KioskApp.Ek.Catalog.Categories
{
    public sealed partial class CarManufacturerPresenter : UserControl
    {
        public CarManufacturerPresenter()
        {
            InitializeComponent();
        }

        #region Category

        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(
            nameof(Category), typeof(Category), typeof(CarManufacturerPresenter), new PropertyMetadata(default(Category), CategoryPropertyChangedCallback));

        private static void CategoryPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CarManufacturerPresenter)d).OnCategoryChanged();
        }

        public Category Category
        {
            get => (Category)GetValue(CategoryProperty);
            set => SetValue(CategoryProperty, value);
        }

        #endregion

        #region ManufacturerLogoUrl

        public static readonly DependencyProperty ManufacturerLogoUrlProperty = DependencyProperty.Register(
            nameof(ManufacturerLogoUrl), typeof(string), typeof(CarManufacturerPresenter), new PropertyMetadata(default(string)));

        public string ManufacturerLogoUrl
        {
            get => (string)GetValue(ManufacturerLogoUrlProperty);
            set => SetValue(ManufacturerLogoUrlProperty, value);
        }

        #endregion

        private void OnCategoryChanged()
        {
            if (Category == null)
            {
                ManufacturerLogoUrl = null;
            }
            else
            {
                ManufacturerLogoUrl = $"/Themes/Assets/Images/Catalog/Model_Logo/{Category.CarManufacturer.Name}.png";
            }
        }

        private void CategoryButton_OnClick(object sender, RoutedEventArgs e)
        {
            OnClick();
        }

        public event EventHandler Click;

        private void OnClick()
        {
            Click?.Invoke(this, EventArgs.Empty);
        }
    }
}