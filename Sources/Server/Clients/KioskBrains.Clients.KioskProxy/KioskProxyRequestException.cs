using System;

namespace KioskBrains.Clients.KioskProxy
{
    public class KioskProxyRequestException : Exception
    {
        public KioskProxyRequestException(string message)
            : base(message)
        {
        }

        public KioskProxyRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}