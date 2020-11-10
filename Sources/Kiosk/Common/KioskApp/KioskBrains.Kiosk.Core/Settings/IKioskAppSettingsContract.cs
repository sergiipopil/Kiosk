using KioskBrains.Kiosk.Core.Components.Contracts;
using KioskBrains.Kiosk.Core.Components.Statuses;

namespace KioskBrains.Kiosk.Core.Settings
{
    public interface IKioskAppSettingsContract : IComponentContract
    {
        BindableComponentStatus Status { get; }

        KioskAppSettingsState State { get; }
    }
}