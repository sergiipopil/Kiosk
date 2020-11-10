using KioskBrains.Kiosk.Core.Components.Operations;

namespace KioskBrains.Kiosk.Core.Components.Initialization
{
    public class ComponentInitializeResponse : IComponentOperationResponse
    {
        public ComponentOperationStatusEnum Status { get; set; }

        public static ComponentInitializeResponse GetSuccess()
        {
            return new ComponentInitializeResponse()
                {
                    Status = ComponentOperationStatusEnum.Success,
                };
        }

        public static ComponentInitializeResponse GetError()
        {
            return new ComponentInitializeResponse()
                {
                    Status = ComponentOperationStatusEnum.Error,
                };
        }
    }
}