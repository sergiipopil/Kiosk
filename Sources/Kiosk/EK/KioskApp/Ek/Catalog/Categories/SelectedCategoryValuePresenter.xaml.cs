using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskApp.Search;

namespace KioskApp.Ek.Catalog.Categories
{
    public sealed partial class SelectedCategoryValuePresenter : UserControl
    {
        public SelectedCategoryValuePresenter()
        {
            InitializeComponent();
        }

        #region Placeholder

        public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
            nameof(Placeholder), typeof(string), typeof(SelectedCategoryValuePresenter), new PropertyMetadata(default(string)));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        #endregion

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(SelectedCategoryValue), typeof(SelectedCategoryValuePresenter), new PropertyMetadata(default(SelectedCategoryValue)));

        public SelectedCategoryValue Value
        {
            get => (SelectedCategoryValue)GetValue(ValueProperty);
            set
            {
                SetValue(ValueProperty, value);
                blockManufacture.Visibility = txtHeader.Visibility = String.IsNullOrEmpty(value.SelectedManufactureURL) ? Visibility.Collapsed : Visibility.Visible;
                blockModel.Visibility = String.IsNullOrEmpty(value.SelectedCarModelText) ? Visibility.Collapsed : Visibility.Visible;
                blockCategory.Visibility = String.IsNullOrEmpty(value.SelectedGroupText) ? Visibility.Collapsed : Visibility.Visible;
                blockSubCategory.Visibility = String.IsNullOrEmpty(value.SelectedSubGroupText) ? Visibility.Collapsed : Visibility.Visible;
                blockSecondSubCategory.Visibility = String.IsNullOrEmpty(value.SelectedSecondSubGroupText) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        #endregion

        private void ChangeButton_OnClick(object sender, RoutedEventArgs e)
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