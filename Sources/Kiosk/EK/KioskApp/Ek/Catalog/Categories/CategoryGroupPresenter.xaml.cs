using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KioskApp.Ek.Catalog.Categories
{
    public sealed partial class CategoryGroupPresenter : UserControl
    {
        public CategoryGroupPresenter()
        {
            OnOperationChanged();

            InitializeComponent();
        }

        #region Category

        public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register(
            nameof(Category), typeof(Category), typeof(CategoryGroupPresenter), new PropertyMetadata(default(Category)));

        public Category Category
        {
            get => (Category)GetValue(CategoryProperty);
            set => SetValue(CategoryProperty, value);
        }

        #endregion

        #region Operation

        public static readonly DependencyProperty OperationProperty = DependencyProperty.Register(
            nameof(Operation), typeof(CategoryGroupOperationEnum), typeof(CategoryGroupPresenter), new PropertyMetadata(default(CategoryGroupOperationEnum), OperationPropertyChangedCallback));

        private static void OperationPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CategoryGroupPresenter)d).OnOperationChanged();
        }

        public CategoryGroupOperationEnum Operation
        {
            get => (CategoryGroupOperationEnum)GetValue(OperationProperty);
            set => SetValue(OperationProperty, value);
        }

        #endregion

        #region ShowExpandLabel

        public static readonly DependencyProperty ShowExpandLabelProperty = DependencyProperty.Register(
            nameof(ShowExpandLabel), typeof(bool), typeof(CategoryGroupPresenter), new PropertyMetadata(default(bool)));

        public bool ShowExpandLabel
        {
            get => (bool)GetValue(ShowExpandLabelProperty);
            set => SetValue(ShowExpandLabelProperty, value);
        }

        #endregion

        #region ShowChangeLabel

        public static readonly DependencyProperty ShowChangeLabelProperty = DependencyProperty.Register(
            nameof(ShowChangeLabel), typeof(bool), typeof(CategoryGroupPresenter), new PropertyMetadata(default(bool)));

        public bool ShowChangeLabel
        {
            get => (bool)GetValue(ShowChangeLabelProperty);
            set => SetValue(ShowChangeLabelProperty, value);
        }

        #endregion

        #region DetailsGroupUrl

        public static readonly DependencyProperty DetailsGroupUrlProperty = DependencyProperty.Register(
            nameof(DetailsGroupUrl), typeof(string), typeof(CarManufacturerPresenter), new PropertyMetadata(default(string)));

        public string DetailsGroupUrl
        {
            get => (string)GetValue(DetailsGroupUrlProperty);
            set => SetValue(DetailsGroupUrlProperty, value);
        }

        #endregion
        private void OnOperationChanged()
        {
            ShowExpandLabel = false;
            ShowChangeLabel = false;

            switch (Operation)
            {
                case CategoryGroupOperationEnum.Expand:
                    ShowExpandLabel = true;
                    break;
                case CategoryGroupOperationEnum.Change:
                    ShowChangeLabel = true;
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