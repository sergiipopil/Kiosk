using System;

namespace KioskBrains.Clients.TecDocWs.Models
{
    public static class ConversionHelper
    {
        public static DateTime? ConvertStringToYearMonth(string source)
        {
            if (source == null
                || source.Length != 6)
            {
                return null;
            }

            try
            {
                var year = int.Parse(source.Substring(0, 4));
                var month = int.Parse(source.Substring(4, 2));
                return new DateTime(year, month, 1);
            }
            catch
            {
                return null;
            }
        }
    }
}