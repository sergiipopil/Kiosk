using System;

namespace KioskBrains.Waf.Security
{
    public class WafUserContractException : Exception
    {
        public WafUserContractException(string message)
            : base(message)
        {
        }
    }
}