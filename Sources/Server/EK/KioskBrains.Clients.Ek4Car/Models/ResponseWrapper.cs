namespace KioskBrains.Clients.Ek4Car.Models
{
    public class ResponseWrapper<TData>
        where TData : class, new()
    {
        public bool success { get; set; }

        public TData data { get; set; }

        public ResponseErrorInfo error { get; set; }
    }
}