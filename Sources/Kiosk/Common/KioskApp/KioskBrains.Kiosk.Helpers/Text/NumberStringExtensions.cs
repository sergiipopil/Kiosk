using System;
using System.Globalization;

namespace KioskBrains.Kiosk.Helpers.Text
{
    public static class NumberStringExtensions
    {
        public static string ToInvariantString(this decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string ToAmountString(this decimal value)
        {
            return ToAmountString(value, removeFractionIfZeros: true);
        }

        public static string ToAmountString(this decimal value, bool removeFractionIfZeros)
        {
            var amountString = Math.Round(value, 2).ToString("N2", CultureInfo.InvariantCulture);
            if (amountString.EndsWith(".00")
                && removeFractionIfZeros)
            {
                amountString = amountString.Substring(0, amountString.Length - 3);
            }
            return amountString;
        }
    }
}