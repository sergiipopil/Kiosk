using System;
using KioskBrains.Common.Logging;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalLogRecordSearch
{
    public class PortalLogRecordSearchRecord
    {
        public int Id { get; set; }

        public int KioskId { get; set; }

        public string KioskVersion { get; set; }

        public DateTime LocalTime { get; set; }

        public LogTypeEnum Type { get; set; }

        public string TypeDisplayName { get; set; }

        public LogContextEnum Context { get; set; }

        public string ContextDisplayName { get; set; }

        public string Message { get; set; }

        public string AdditionalDataJson { get; set; }
    }
}