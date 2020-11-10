using System;

namespace KioskBrains.Kiosk.Core.Components
{
    public class ComponentLockedException : Exception
    {
        public ComponentLockedException(string componentName)
            : base($"Component '{componentName}' is locked.")
        {
        }
    }
}