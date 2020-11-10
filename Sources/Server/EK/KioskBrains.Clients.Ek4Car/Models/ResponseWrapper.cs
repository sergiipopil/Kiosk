namespace KioskBrains.Clients.Ek4Car.Models
{
    public class ResponseWrapper<TData>
        where TData : class, new()
    {
        public bool Success { get; set; }

        public TData Data { get; set; }

        public ResponseErrorInfo Error { get; set; }
    }
}