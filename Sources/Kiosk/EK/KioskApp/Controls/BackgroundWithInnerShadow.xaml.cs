using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace KioskApp.Controls
{
    public sealed partial class BackgroundWithInnerShadow : UserControl
    {
        public BackgroundWithInnerShadow()
        {
            InitializeComponent();
        }

        #region BlurRadius

        public static readonly DependencyProperty BlurRadiusProperty = DependencyProperty.Register(
            nameof(BlurRadius), typeof(double), typeof(BackgroundWithInnerShadow), new PropertyMetadata(5d));

        public double BlurRadius
        {
            get => (double)GetValue(BlurRadiusProperty);
            set => SetValue(BlurRadiusProperty, value);
        }

        #endregion

        #region ShadowOpacity

        public static readonly DependencyProperty ShadowOpacityProperty = DependencyProperty.Register(
            nameof(ShadowOpacity), typeof(double), typeof(BackgroundWithInnerShadow), new PropertyMetadata(0.9d));

        public double ShadowOpacity
        {
            get => (double)GetValue(ShadowOpacityProperty);
            set => SetValue(ShadowOpacityProperty, value);
        }

        #endregion

        #region ShadowColor

        public static readonly DependencyProperty ShadowColorProperty = DependencyProperty.Register(
            nameof(ShadowColor), typeof(Color), typeof(BackgroundWithInnerShadow), new PropertyMetadata(Colors.Black));

        public Color ShadowColor
        {
            get => (Color)GetValue(ShadowColorProperty);
            set => SetValue(ShadowColorProperty, value);
        }

        #endregion

        #region OuterBorderBrush

        public static readonly DependencyProperty OuterBorderBrushProperty = DependencyProperty.Register(
            nameof(OuterBorderBrush), typeof(Brush), typeof(BackgroundWithInnerShadow), new PropertyMetadata(default(Brush)));

        public Brush OuterBorderBrush
        {
            get => (Brush)GetValue(OuterBorderBrushProperty);
            set => SetValue(OuterBorderBrushProperty, value);
        }

        #endregion

        #region OuterBorderThickness

        public static readonly DependencyProperty OuterBorderThicknessProperty = DependencyProperty.Register(
            nameof(OuterBorderThickness), typeof(double), typeof(BackgroundWithInnerShadow), new PropertyMetadata(5d));

        public double OuterBorderThickness
        {
            get => (double)GetValue(OuterBorderThicknessProperty);
            set => SetValue(OuterBorderThicknessProperty, value);
        }

        #endregion
    }
}