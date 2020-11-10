using KioskBrains.Kiosk.Core.Components.Statuses;

namespace KioskBrains.Kiosk.Core.Components
{
    public class ServiceMonitorableComponentInfo
    {
        public string FullName { get; set; }

        public BindableComponentStatus Status { get; set; }
    }
}