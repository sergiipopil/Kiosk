using System;

namespace KioskBrains.Clients.OmegaAutoBiz
{
    public class OmegaAutoBizRequestException : Exception
    {
        public OmegaAutoBizRequestException(string message)
            : base(message)
        {
        }

        public OmegaAutoBizRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}