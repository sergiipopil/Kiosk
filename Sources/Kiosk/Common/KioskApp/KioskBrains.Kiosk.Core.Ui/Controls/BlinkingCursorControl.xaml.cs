using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace KioskBrains.Kiosk.Core.Ui.Controls
{
    public sealed partial class BlinkingCursorControl : UserControl
    {
        public BlinkingCursorControl()
        {
            InitializeComponent();

            RegisterPropertyChangedCallback(VisibilityProperty, VisibilityChangedCallback);
            OnVisibilityChanged();
        }

        private void VisibilityChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            ((BlinkingCursorControl)sender).OnVisibilityChanged();
        }

        private void OnVisibilityChanged()
        {
            switch (Visibility)
            {
                case Visibility.Visible:
                    CursorElementBlinkingAnimation.Begin();
                    break;
                case Visibility.Collapsed:
                    CursorElementBlinkingAnimation.Stop();
                    break;
            }
        }

        #region Fill

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            nameof(Fill), typeof(Brush), typeof(BlinkingCursorControl), new PropertyMetadata(default(Brush)));

        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        #endregion
    }
}