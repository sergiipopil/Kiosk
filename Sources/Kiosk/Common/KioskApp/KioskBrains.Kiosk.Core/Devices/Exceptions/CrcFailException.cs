using System;

namespace KioskBrains.Kiosk.Core.Devices.Exceptions
{
    public class CrcFailException : Exception
    {
        public CrcFailException(string message)
            : base(message)
        {
        }
    }
}