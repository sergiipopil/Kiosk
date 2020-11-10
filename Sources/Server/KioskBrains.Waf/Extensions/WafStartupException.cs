using System;

namespace KioskBrains.Waf.Extensions
{
    public class WafStartupException : Exception
    {
        public WafStartupException(string message)
            : base(message)
        {
        }
    }
}