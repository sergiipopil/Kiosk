using System;
using KioskBrains.Common.Helpers;
using KioskBrains.Common.Logging;

namespace KioskBrains.KioskAutoUpdater.ServerApi
{
    public class ServerApiException : Exception, ILoggableObject
    {
        public ServerApiException(string message)
            : base(message)
        {
        }

        public ServerApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public object GetLogObject()
        {
            return new
                {
                    Messages = this.GetStackMessages(),
                };
        }
    }
}