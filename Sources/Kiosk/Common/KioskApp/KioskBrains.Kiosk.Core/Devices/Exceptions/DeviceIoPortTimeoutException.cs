using System;

namespace KioskBrains.Kiosk.Core.Devices.Exceptions
{
    public class DeviceIoPortTimeoutException : Exception
    {
        public bool PartialRead { get; }

        public DeviceIoPortTimeoutException()
        {
        }

        public DeviceIoPortTimeoutException(bool partialRead)
            : base("Partial read!")
        {
            PartialRead = partialRead;
        }
    }
}