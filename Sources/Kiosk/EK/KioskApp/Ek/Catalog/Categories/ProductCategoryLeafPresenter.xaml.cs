using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KioskApp.Ek.Catalog.Categories
{
    public sealed partial class ProductCategoryLeafPresenter : UserControl
    {
        public ProductCategoryLeafPresenter()
        {
            InitializeComponent();
        }

        #region Category

        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(
            nameof(Category), typeof(Category), typeof(ProductCategoryLeafPresenter), new PropertyMetadata(default(Category), CategoryPropertyChangedCallback));

        private static void CategoryPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProductCategoryLeafPresenter)d).OnCategoryChanged();
        }

        public Category Category
        {
            get => (Category)GetValue(CategoryProperty);
            set => SetValue(CategoryProperty, value);
        }

        #endregion

        #region SpecialTypeText

        public static readonly DependencyProperty SpecialTypeTextProperty = DependencyProperty.Register(
            nameof(SpecialTypeText), typeof(string), typeof(ProductCategoryLeafPresenter), new PropertyMetadata(default(string)));

        public string SpecialTypeText
        {
            get => (string)GetValue(SpecialTypeTextProperty);
            set => SetValue(SpecialTypeTextProperty, value);
        }

        #endregion

        private void OnCategoryChanged()
        {
            UpdateSpecialTypeText();
        }

        private void UpdateSpecialTypeText()
        {
            switch (Category?.SpecialType)
            {
                case CategorySpecialTypeEnum.ProductCategoryGroupSelector:
                    SpecialTypeText = " (ВСЕ)";
                    break;
                default:
                    SpecialTypeText = null;
                    break;
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