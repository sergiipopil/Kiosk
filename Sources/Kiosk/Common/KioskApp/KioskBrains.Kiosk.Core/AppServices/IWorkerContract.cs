using KioskBrains.Kiosk.Core.Components.Contracts;
using KioskBrains.Kiosk.Core.Components.Statuses;

namespace KioskBrains.Kiosk.Core.AppServices
{
    public interface IWorkerContract : IComponentContract
    {
        BindableComponentStatus Status { get; }
    }
}