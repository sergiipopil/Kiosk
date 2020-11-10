using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace KioskApp.Ek.Catalog.Categories
{
    public sealed partial class TopCategoryPresenter : UserControl
    {
        public TopCategoryPresenter()
        {
            InitializeComponent();
        }

        public event EventHandler<EventArgs> Click;

        private void OnClick()
        {
            Click?.Invoke(this, EventArgs.Empty);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            OnClick();
        }

        #region Title

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(TopCategoryPresenter), new PropertyMetadata(default(string)));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        #endregion

        #region Icon

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon), typeof(ImageSource), typeof(TopCategoryPresenter), new PropertyMetadata(default(ImageSource)));

        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        #endregion
    }
}