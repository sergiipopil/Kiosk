using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskApp.Search;

namespace KioskApp.Ek.Catalog.Categories
{
    public sealed partial class CarModelModificationCategoryPresenter : UserControl
    {
        public CarModelModificationCategoryPresenter()
        {
            InitializeComponent();
        }

        #region Category

        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(
            nameof(Category), typeof(Category), typeof(CarModelModificationCategoryPresenter), new PropertyMetadata(default(Category), CategoryPropertyChangedCallback));

        private static void CategoryPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CarModelModificationCategoryPresenter)d).OnCategoryChanged();
        }

        public Category Category
        {
            get => (Category)GetValue(CategoryProperty);
            set => SetValue(CategoryProperty, value);
        }

        #endregion

        #region CarModelModification

        public static readonly DependencyProperty CarModelModificationProperty = DependencyProperty.Register(
            nameof(CarModelModification), typeof(CarModelModification), typeof(CarModelModificationCategoryPresenter), new PropertyMetadata(default(CarModelModification)));

        public CarModelModification CarModelModification
        {
            get => (CarModelModification)GetValue(CarModelModificationProperty);
            set => SetValue(CarModelModificationProperty, value);
        }

        #endregion

        private void OnCategoryChanged()
        {
            CarModelModification = Category == null
                ? null
                : new CarModelModification(Category.CarModelModification);
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