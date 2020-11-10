using System;

namespace KioskBrains.Server.Common.Helpers
{
    public static class TimestampHelper
    {
        public static long GetCurrentUtcTotalMinutes()
        {
            var timeSpan = new TimeSpan(DateTime.UtcNow.Ticks);
            var totalMinutes = (int)timeSpan.TotalMinutes;
            return totalMinutes;
        }
    }
}