using System;

namespace KioskBrains.Waf.Managers.Processing
{
    public class WafManagerContractException : Exception
    {
        public WafManagerContractException(string message)
            : base(message)
        {
        }
    }
}