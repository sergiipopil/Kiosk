using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace KioskApp.Controls
{
    public sealed partial class MenuPointControl : UserControl
    {
        public MenuPointControl()
        {
            InitializeComponent();
        }

        #region Icon

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon), typeof(ImageSource), typeof(MenuPointControl), new PropertyMetadata(default(ImageSource)));

        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        #endregion

        #region Label

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(MenuPointControl), new PropertyMetadata(default(string)));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        #endregion

        #region ButtonStyle

        public static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register(
            nameof(ButtonStyle), typeof(Style), typeof(MenuPointControl), new PropertyMetadata(default(Style)));

        public Style ButtonStyle
        {
            get => (Style)GetValue(ButtonStyleProperty);
            set => SetValue(ButtonStyleProperty, value);
        }

        #endregion

        public string Id { get; set; }

        public event EventHandler<MenuPointClickedEventArgs> MenuPointClicked;

        private void OnMenuPointClicked(MenuPointClickedEventArgs e)
        {
            MenuPointClicked?.Invoke(this, e);
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            OnMenuPointClicked(new MenuPointClickedEventArgs(Id));
        }
    }
}