using System;
using System.Runtime.CompilerServices;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Helpers;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Devices.Helpers;

namespace KioskBrains.Kiosk.Core.Components.Operations
{
    public class ComponentOperationLogger : IDisposable
    {
        private readonly string _componentFullName;

        private readonly string _operationName;

        public ComponentOperationLogger(string componentFullName, string operationName)
        {
            Assure.ArgumentNotNull(componentFullName, nameof(componentFullName));
            Assure.ArgumentNotNull(operationName, nameof(operationName));

            _componentFullName = componentFullName;
            _operationName = operationName;
        }

        private readonly TraceBuilder _traceBuilder = new TraceBuilder();

        public void AddTrace(string message)
        {
            _traceBuilder.AddTrace(message);
        }

        public void FlushTrace()
        {
            // write trace if not empty
            if (!_traceBuilder.IsEmpty)
            {
                Write(LogTypeEnum.Trace, LogContextEnum.Component, "Trace.", _traceBuilder.GetLogObject(), callerName: null);
                _traceBuilder.Reset();
            }
        }

        public void Info(LogContextEnum context, string message, object additionalData = null, [CallerMemberName] string callerName = null)
        {
            Write(LogTypeEnum.Info, context, message, additionalData, callerName);
        }

        public void Warning(LogContextEnum context, string message, object additionalData = null, [CallerMemberName] string callerName = null)
        {
            Write(LogTypeEnum.Warning, context, message, additionalData, callerName);
        }

        public void Error(LogContextEnum context, string message, object additionalData = null, [CallerMemberName] string callerName = null)
        {
            Write(LogTypeEnum.Error, context, message, additionalData, callerName);
        }

        public void Error(LogContextEnum context, Exception exception, [CallerMemberName] string callerName = null)
        {
            Error(context, exception?.Message, exception);
        }

        public void Error(LogContextEnum context, string message, Exception exception, [CallerMemberName] string callerName = null)
        {
            Write(LogTypeEnum.Error, context, message, exception.GetLoggableObject(), callerName);
        }

        private void Write(LogTypeEnum type, LogContextEnum context, string message, object additionalData, string callerName)
        {
            var callerNamePrefix = string.IsNullOrEmpty(callerName)
                                   || callerName == _operationName
                                   || callerName == _operationName + "Async"
                ? null
                : $"{callerName}: ";

            // modify message with 
            message = $"'{_componentFullName}'.{_operationName}: {callerNamePrefix}{message}";

            Log.Write(type, context, message, additionalData);
        }

        public void Dispose()
        {
            FlushTrace();
        }
    }
}