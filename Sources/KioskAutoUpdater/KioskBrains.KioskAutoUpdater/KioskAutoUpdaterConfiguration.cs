using System;

namespace KioskBrains.KioskAutoUpdater
{
    public class KioskAutoUpdaterConfiguration
    {
        public Uri ServerUri { get; set; }

        public TimeSpan UpdateCheckInterval { get; set; }

        public TimeSpan MaxNotReadyForUpdateLockDuration { get; set; }

        public TimeSpan ReadyForUpdateCheckInterval { get; set; }
    }
}