using System;

namespace KioskBrains.Waf.Actions.Processing
{
    public class WafActionContractException : Exception
    {
        public WafActionContractException(string message)
            : base(message)
        {
        }
    }
}