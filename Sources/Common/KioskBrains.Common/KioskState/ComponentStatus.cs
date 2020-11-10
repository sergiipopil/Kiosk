using System;

namespace KioskBrains.Common.KioskState
{
    public class ComponentStatus
    {
        public ComponentStatusCodeEnum Code { get; set; }

        public string Message { get; set; }

        public DateTime StatusLocalTime { get; set; }
    }
}