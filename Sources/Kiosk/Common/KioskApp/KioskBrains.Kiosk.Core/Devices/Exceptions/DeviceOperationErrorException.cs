using System;

namespace KioskBrains.Kiosk.Core.Devices.Exceptions
{
    public class DeviceOperationErrorException : Exception
    {
        public DeviceOperationErrorException(string message)
            : base(message)
        {
        }

        public DeviceOperationErrorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}