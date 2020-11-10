using System;

namespace KioskBrains.Kiosk.Core.Components.Operations
{
    public class ComponentOperationException : Exception
    {
        public ComponentOperationException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}