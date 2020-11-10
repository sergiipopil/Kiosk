namespace KioskBrains.Proxy.Common.Models
{
    public class PassResponse
    {
        public bool IsReceived { get; set; }

        public string ErrorMessage { get; set; }

        public HttpResponseData ReceivedResponse { get; set; }
    }
}