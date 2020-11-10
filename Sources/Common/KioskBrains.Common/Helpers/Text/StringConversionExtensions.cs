using System.Globalization;

namespace KioskBrains.Common.Helpers.Text
{
    public static class StringConversionExtensions
    {
        public static int? ParseIntOrNull(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return int.TryParse(value, out var parsedValue)
                ? parsedValue
                : (int?)null;
        }

        public static decimal? ParseDecimalOrNull(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return decimal.TryParse(value, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out var parsedValue)
                ? parsedValue
                : (decimal?)null;
        }
    }
}