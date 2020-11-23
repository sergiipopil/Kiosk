using KioskApp.CoreExtension.Application;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KioskApp.Ek.Catalog.Categories
{
    public sealed partial class CarModelPresenter : UserControl
    {
        public CarModelPresenter()
        {
            InitializeComponent();
        }

        #region Category

        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(
            nameof(Category), typeof(Category), typeof(CarModelPresenter), new PropertyMetadata(default(Category), SubCategoryPropertyChangedCallback));

        private static void SubCategoryPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CarModelPresenter)d).OnCategoryChanged();
        }
        public Category Category
        {
            get => (Category)GetValue(CategoryProperty);
            set => SetValue(CategoryProperty, value);
        }

        #endregion

        private void CategoryButton_OnClick(object sender, RoutedEventArgs e)
        {
            OnClick();
        }

        public event EventHandler Click;

        private void OnClick()
        {
            Click?.Invoke(this, EventArgs.Empty);
        }
        #region CarModelLogoUrl

        public static readonly DependencyProperty CarModelLogoUrlProperty = DependencyProperty.Register(
            nameof(CarModelLogoUrlProperty), typeof(string), typeof(CarManufacturerPresenter), new PropertyMetadata(default(string)));

        public string CarModelLogoUrl
        {
            get => (string)GetValue(CarModelLogoUrlProperty);
            set => SetValue(CarModelLogoUrlProperty, value);
        }

        #endregion

        private void OnCategoryChanged()
        {
            if (Category == null)
            {
                CarModelLogoUrl = null;
            }
            else
            {
                var manufacturer = EkSettingsHelper.GetModelManufacturerNameByModelId(Category.Id.ToString());
                CarModelLogoUrl = $"/Themes/Assets/Images/Catalog/CarModel/{manufacturer}/{Category.Name}.png";
            }
        }
    }
}