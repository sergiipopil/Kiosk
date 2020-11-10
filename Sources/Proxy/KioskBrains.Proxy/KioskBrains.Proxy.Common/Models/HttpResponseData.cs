using System.Collections.Generic;

namespace KioskBrains.Proxy.Common.Models
{
    public class HttpResponseData
    {
        public int StatusCode { get; set; }

        public Dictionary<string, string> ResponseHeaders { get; set; }

        public Dictionary<string, string> ContentHeaders { get; set; }

        public string ContentBody { get; set; }
    }
}