namespace KioskBrains.Kiosk.Core.Components.Operations
{
    public class BasicOperationResponse : IComponentOperationResponse
    {
        public ComponentOperationStatusEnum Status { get; set; }

        public static BasicOperationResponse GetSuccess()
        {
            return new BasicOperationResponse()
                {
                    Status = ComponentOperationStatusEnum.Success,
                };
        }

        public static BasicOperationResponse GetError()
        {
            return new BasicOperationResponse()
                {
                    Status = ComponentOperationStatusEnum.Error,
                };
        }
    }
}