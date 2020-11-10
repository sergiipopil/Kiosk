using System;

namespace KioskBrains.Kiosk.Helpers.Dates
{
    public static class DateTimeExtensions
    {
        // https://msdn.microsoft.com/en-us/library/9kkf9tah.aspx?f=255&MSPPError=-2147217396
        public static uint ToDosDateTime(this DateTime dateTime)
        {
            var year = (uint)(dateTime.Year - 1980); // relative to 1980
            if (year > 119)
            {
                throw new ArgumentOutOfRangeException(nameof(dateTime), "DOS format year value is bigger than 119.");
            }
            var month = (uint)dateTime.Month;
            var day = (uint)dateTime.Day;
            var hours = (uint)dateTime.Hour;
            var minutes = (uint)dateTime.Minute;
            var seconds = (uint)dateTime.Second >> 1; // each 2nd second

            var date = (year << 9) | (month << 5) | day;
            var time = (hours << 11) | (minutes << 5) | seconds;

            return (date << 16) | time;
        }

        public static DateTime FromDosDateTime(this uint dosDateTime)
        {
            var date = (dosDateTime & 0xFFFF0000) >> 16;
            var time = dosDateTime & 0x0000FFFF;

            var year = (date >> 9) + 1980;
            var month = (date & 0x01E0) >> 5;
            var day = date & 0x1F;
            var hour = time >> 11;
            var minute = (time & 0x07E0) >> 5;
            var second = (time & 0x1F) * 2;

            return new DateTime((int)year, (int)month, (int)day, (int)hour, (int)minute, (int)second);
        }
    }
}