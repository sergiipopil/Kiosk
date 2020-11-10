using System;

namespace KioskBrains.Kiosk.Core.Devices.Exceptions
{
    public class DeviceNonOperationalStateException : Exception
    {
        public DeviceNonOperationalStateException(byte code, string message)
            : this($"0x{code:X2}", message)
        {
        }

        public DeviceNonOperationalStateException(string code, string message)
            : this($"{message} ({code}).")
        {
        }

        public DeviceNonOperationalStateException(string message)
            : this(message, (Exception)null)
        {
        }

        public DeviceNonOperationalStateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}