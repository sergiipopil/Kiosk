using Windows.UI.Xaml.Controls;

namespace KioskBrains.Kiosk.Core.Ui.Controls
{
    /// <summary>
    /// Added to fix the issue with FontSize and FontWeight (at the moment it's set as local value, so it's not inherited from button).
    /// </summary>
    public class FontIconFixed : FontIcon
    {
        public FontIconFixed()
        {
            ClearValue(FontSizeProperty);
            ClearValue(FontWeightProperty);
        }
    }
}