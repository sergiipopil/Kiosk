using System;

namespace KioskBrains.Common.Logging
{
    public class LogRecord
    {
        public string UniqueId { get; set; }

        public string KioskVersion { get; set; }

        public DateTime LocalTime { get; set; }

        public LogTypeEnum Type { get; set; }

        public LogContextEnum Context { get; set; }

        public string Message { get; set; }

        public string AdditionalDataJson { get; set; }
    }
}