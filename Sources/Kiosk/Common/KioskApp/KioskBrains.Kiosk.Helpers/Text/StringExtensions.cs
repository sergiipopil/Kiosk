using System;
using System.Linq;
using KioskBrains.Common.Contracts;

namespace KioskBrains.Kiosk.Helpers.Text
{
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string value)
        {
            Assure.ArgumentNotNull(value, nameof(value));

            if (value.Length == 0)
            {
                return value;
            }

            return value.First().ToString().ToUpper() + value.Substring(1);
        }

        public static string FirstNSymbols(this string value, int n)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.Substring(0, Math.Min(n, value.Length));
        }
    }
}