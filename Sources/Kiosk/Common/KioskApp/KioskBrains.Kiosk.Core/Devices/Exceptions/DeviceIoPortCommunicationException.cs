using System;

namespace KioskBrains.Kiosk.Core.Devices.Exceptions
{
    public class DeviceIoPortCommunicationException : Exception
    {
        public DeviceIoPortCommunicationException(string message)
            : this(message, null)
        {
        }

        public DeviceIoPortCommunicationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}