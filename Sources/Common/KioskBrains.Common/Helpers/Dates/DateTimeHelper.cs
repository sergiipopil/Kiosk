using System;

namespace KioskBrains.Common.Helpers.Dates
{
    public static class DateTimeHelper
    {
        public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public static string ToDateTimeString(this DateTime value)
        {
            return value.ToString(DateTimeFormat);
        }

        public static string ToDateTimeString(this DateTime? value)
        {
            return value?.ToString(DateTimeFormat);
        }
    }
}