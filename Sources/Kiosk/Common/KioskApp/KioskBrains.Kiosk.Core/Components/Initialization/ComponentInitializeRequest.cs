using KioskBrains.Common.KioskConfiguration;

namespace KioskBrains.Kiosk.Core.Components.Initialization
{
    public class ComponentInitializeRequest
    {
        public ComponentSettings Settings { get; }

        public ComponentInitializeRequest(ComponentSettings settings)
        {
            Settings = settings;
        }
    }
}