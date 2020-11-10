using System;
using KioskBrains.Common.KioskConfiguration;
using KioskBrains.Kiosk.Core.Components.States;

namespace KioskBrains.Kiosk.Core.Settings
{
    public class KioskAppSettingsState : ComponentState
    {
        public string KioskAppVersion { get; set; }

        public Uri ServerUri { get; set; }

        public KioskConfiguration KioskConfiguration { get; set; }

        public int KioskId { get; set; }

        public string SupportPhone { get; set; }

        public KioskAddress KioskAddress { get; set; }
    }
}