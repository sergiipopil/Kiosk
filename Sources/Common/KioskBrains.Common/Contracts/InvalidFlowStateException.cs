using System;

namespace KioskBrains.Common.Contracts
{
    public class InvalidFlowStateException : Exception
    {
        public InvalidFlowStateException(string message)
            : base(message)
        {
        }
    }
}
