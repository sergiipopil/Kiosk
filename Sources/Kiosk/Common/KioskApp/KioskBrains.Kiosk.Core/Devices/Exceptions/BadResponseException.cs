using System;

namespace KioskBrains.Kiosk.Core.Devices.Exceptions
{
    public class BadResponseException : Exception
    {
        public BadResponseException()
        {
        }

        public BadResponseException(string message)
            : base(message)
        {
        }
    }
}