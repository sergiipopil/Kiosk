using KioskBrains.Kiosk.Helpers.Text;

namespace KioskApp.Helpers
{
    public static class NumberStringExtensions
    {
        public static string ToAmountStringWithSpaces(this decimal value)
        {
            return value.ToAmountString().Replace(",", " ");
        }
    }
}