using Windows.UI.Xaml;

namespace KioskApp.Controls
{
    public class ButtonEx : DependencyObject
    {
        #region TopBorderOpacity (Attached)

        public static readonly DependencyProperty TopBorderOpacityProperty = DependencyProperty.RegisterAttached(
            "TopBorderOpacity", typeof(double), typeof(ButtonEx), new PropertyMetadata(default(double)));

        public static void SetTopBorderOpacity(DependencyObject element, double value)
        {
            element.SetValue(TopBorderOpacityProperty, value);
        }

        public static double GetTopBorderOpacity(DependencyObject element)
        {
            return (double)element.GetValue(TopBorderOpacityProperty);
        }

        #endregion
    }
}