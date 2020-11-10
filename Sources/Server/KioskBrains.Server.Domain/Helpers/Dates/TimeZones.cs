using System;
using KioskBrains.Server.Domain.Security;

namespace KioskBrains.Server.Domain.Helpers.Dates
{
    public static class TimeZones
    {
        public const string PacificStandardTime = "Pacific Standard Time";
        public const string EasternStandardTime = "Eastern Standard Time";
        public const string GmtStandardTime = "GMT Standard Time";
        public const string UtcTime = "UTC";
        public const string MoldovaTime = "E. Europe Standard Time";
        public const string UkrainianTime = "FLE Standard Time";
        public const string UzbekistanTime = "West Asia Standard Time";
        public const string KazakhstanTime = "Central Asia Standard Time";

        public static DateTime GetCustomerNow(CurrentUser currentUser)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(currentUser.TimeZoneName ?? UkrainianTime));
        }

        public static DateTime GetTimeZoneNow(string timeZone)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(timeZone));
        }
    }
}