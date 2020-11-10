using Windows.Foundation;

namespace KioskBrains.Kiosk.Helpers.Graphics
{
    public static class MeasurementHelper
    {
        public static Point Center(this Rect rect)
        {
            return new Point(rect.X + rect.Width/2, rect.Y + rect.Height/2);
        }
    }
}