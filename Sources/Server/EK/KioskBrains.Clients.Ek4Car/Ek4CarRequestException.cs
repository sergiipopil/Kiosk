using System;

namespace KioskBrains.Clients.Ek4Car
{
    public class Ek4CarRequestException : Exception
    {
        public Ek4CarRequestException(string message)
            : base(message)
        {
        }

        public Ek4CarRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}