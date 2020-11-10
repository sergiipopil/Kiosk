using System;

namespace KioskBrains.KioskAutoUpdater.Helpers
{
    public static class StringExtensions
    {
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