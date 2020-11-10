using System;

namespace KioskBrains.Kiosk.Core.Components.Initialization
{
    public class ComponentConfigurationException : Exception
    {
        public ComponentConfigurationException(string message)
            : base(message)
        {
        }
    }
}