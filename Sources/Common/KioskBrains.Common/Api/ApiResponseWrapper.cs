namespace KioskBrains.Common.Api
{
    public class ApiResponseWrapper<TResponse>
    {
        public ApiResponseMeta Meta { get; set; }

        public TResponse Data { get; set; }
    }
}