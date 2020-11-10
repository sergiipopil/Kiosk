using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Controls
{
    public sealed partial class LoadingControl : UserControl
    {
        public LoadingControl()
        {
            InitializeComponent();

            OnShowOverlayChanged();
        }

        #region Size

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            nameof(Size), typeof(LoadingControlSizeEnum), typeof(LoadingControl), new PropertyMetadata(default(LoadingControlSizeEnum), SizePropertyChangedCallback));

        private static void SizePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LoadingControl)d).OnSizeChanged();
        }

        public LoadingControlSizeEnum Size
        {
            get => (LoadingControlSizeEnum)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        #endregion

        #region ShowOverlay

        public static readonly DependencyProperty ShowOverlayProperty = DependencyProperty.Register(
            nameof(ShowOverlay), typeof(bool), typeof(LoadingControl), new PropertyMetadata(true, ShowOverlayPropertyChangedCallback));

        private static void ShowOverlayPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LoadingControl)d).OnShowOverlayChanged();
        }

        public bool ShowOverlay
        {
            get => (bool)GetValue(ShowOverlayProperty);
            set => SetValue(ShowOverlayProperty, value);
        }

        #endregion

        #region Text

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(LoadingControl), new PropertyMetadata(default(string)));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        #endregion

        private void OnSizeChanged()
        {
            int spinnerSize;
            switch (Size)
            {
                case LoadingControlSizeEnum.Small:
                    spinnerSize = 16;
                    break;
                case LoadingControlSizeEnum.Medium:
                    spinnerSize = 32;
                    break;
                case LoadingControlSizeEnum.Large:
                    spinnerSize = 64;
                    break;
                default:
                    return;
            }

            Spinner.Width = spinnerSize;
            Spinner.Height = spinnerSize;
        }

        private void OnShowOverlayChanged()
        {
            OverlayElement.Background = ShowOverlay
                ? ResourceHelper.GetStaticResourceFromUIThread<Brush>("LoadingOverlayBrush")
                : null;
        }
    }
}