using System;
using System.Text;
using KioskBrains.Common.Logging;

namespace KioskBrains.Common.Helpers
{
    public static class ExceptionExtensions
    {
        public static string GetStackMessages(this Exception exception)
        {
            if (exception == null)
            {
                return null;
            }
            var messageBuilder = new StringBuilder();
            do
            {
                messageBuilder.AppendLine(exception.Message);
                exception = exception.InnerException;
            }
            while (exception != null);
            var message = messageBuilder.ToString().Trim();
            return message;
        }

        public static object GetLoggableObject(this Exception exception)
        {
            if (exception == null)
            {
                return null;
            }

            if (exception is ILoggableObject loggableObjectProvider)
            {
                return loggableObjectProvider.GetLogObject();
            }

            return new
                {
                    Exception = exception.ToString(),
                };
        }
    }
}