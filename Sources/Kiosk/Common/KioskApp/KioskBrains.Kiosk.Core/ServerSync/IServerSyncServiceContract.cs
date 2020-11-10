using KioskBrains.Kiosk.Core.Components.Contracts;
using KioskBrains.Kiosk.Core.Components.Operations;
using KioskBrains.Kiosk.Core.Components.Statuses;

namespace KioskBrains.Kiosk.Core.ServerSync
{
    public interface IServerSyncServiceContract : IComponentContract
    {
        BindableComponentStatus Status { get; }

        ComponentOperation<EmptyOperationRequest, BasicOperationResponse> ScheduleImmediateSync { get; }
    }
}