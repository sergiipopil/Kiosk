using System;

namespace KioskBrains.Kiosk.Core.Devices.Exceptions
{
    public class DeviceIoPortInitializationException : Exception
    {
        public DeviceIoPortInitializationException(string message)
            : base(message)
        {
        }
    }
}