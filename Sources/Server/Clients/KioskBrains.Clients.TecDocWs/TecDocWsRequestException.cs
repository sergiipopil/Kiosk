using System;

namespace KioskBrains.Clients.TecDocWs
{
    public class TecDocWsRequestException : Exception
    {
        public TecDocWsRequestException(string message)
            : base(message)
        {
        }

        public TecDocWsRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}