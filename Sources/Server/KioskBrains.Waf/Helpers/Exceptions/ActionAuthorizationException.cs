using System;

namespace KioskBrains.Waf.Helpers.Exceptions
{
    public class ActionAuthorizationException : Exception
    {
        public ActionAuthorizationException()
        {
        }

        public ActionAuthorizationException(string message)
            : base(message)
        {
        }
    }
}