using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace KioskApp.Ek.Catalog
{
    public sealed partial class AutoPartsSectionPresenter : UserControl
    {
        public AutoPartsSectionPresenter()
        {
            InitializeComponent();
        }

        public event EventHandler<EventArgs> Selected;

        private void OnSelected()
        {
            Selected?.Invoke(this, EventArgs.Empty);
        }

        private void SelectButton_OnClick(object sender, RoutedEventArgs e)
        {
            OnSelected();
        }

        #region HeaderLine1

        public static readonly DependencyProperty HeaderLine1Property = DependencyProperty.Register(
            nameof(HeaderLine1), typeof(string), typeof(AutoPartsSectionPresenter), new PropertyMetadata(default(string)));

        public string HeaderLine1
        {
            get => (string)GetValue(HeaderLine1Property);
            set => SetValue(HeaderLine1Property, value);
        }

        #endregion

        #region HeaderLine2

        public static readonly DependencyProperty HeaderLine2Property = DependencyProperty.Register(
            nameof(HeaderLine2), typeof(string), typeof(AutoPartsSectionPresenter), new PropertyMetadata(default(string)));

        public string HeaderLine2
        {
            get => (string)GetValue(HeaderLine2Property);
            set => SetValue(HeaderLine2Property, value);
        }

        #endregion

        #region Icon

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon), typeof(ImageSource), typeof(AutoPartsSectionPresenter), new PropertyMetadata(default(ImageSource)));

        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        #endregion
    }
}