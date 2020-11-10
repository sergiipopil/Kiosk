using System.Collections.Generic;

namespace KioskBrains.Proxy.Common.Models
{
    public class PassRequest
    {
        public string Url { get; set; }

        public string Method { get; set; }
        
        public Dictionary<string, string> RequestHeaders { get; set; }

        public Dictionary<string, string> ContentHeaders { get; set; }

        public string ContentBody { get; set; }
    }
}