using System;

namespace KioskBrains.Waf.Helpers.Exceptions
{
    public class AuthenticationRequiredException : Exception
    {
        public AuthenticationRequiredException(string message = null)
            : base(message ?? "Authentication is required.")
        {
        }
    }
}