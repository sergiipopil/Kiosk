using KioskBrains.Kiosk.Core.Components.Contracts;
using KioskBrains.Kiosk.Core.Components.Statuses;

namespace KioskBrains.Kiosk.Core.Application
{
    public interface IKioskApplicationContract : IComponentContract
    {
        BindableComponentStatus Status { get; }

        KioskApplicationState State { get; }
    }
}