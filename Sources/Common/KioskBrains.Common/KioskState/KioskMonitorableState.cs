using System;

namespace KioskBrains.Common.KioskState
{
    public class KioskMonitorableState
    {
        public string KioskVersion { get; set; }

        public DateTime LocalTime { get; set; }

        public ComponentMonitorableState[] ComponentMonitorableStates { get; set; }
    }
}