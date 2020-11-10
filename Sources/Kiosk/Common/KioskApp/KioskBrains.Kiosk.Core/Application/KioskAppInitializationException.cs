using System;

namespace KioskBrains.Kiosk.Core.Application
{
    public class KioskAppInitializationException : Exception
    {
        public KioskAppInitializationException(string message)
            : base(message)
        {
        }
    }
}