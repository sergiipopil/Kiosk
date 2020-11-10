using System;
using System.Threading.Tasks;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Helpers;

namespace KioskBrains.Common.Logging
{
    public static class Log
    {
        public static void Trace(LogContextEnum context, string message, object additionalData = null)
        {
            Write(LogTypeEnum.Trace, context, message, additionalData);
        }

        public static void Info(LogContextEnum context, string message, object additionalData = null)
        {
            Write(LogTypeEnum.Info, context, message, additionalData);
        }

        public static void Warning(LogContextEnum context, string message, object additionalData = null)
        {
            Write(LogTypeEnum.Warning, context, message, additionalData);
        }

        public static void Error(LogContextEnum context, string message, object additionalData = null)
        {
            Write(LogTypeEnum.Error, context, message, additionalData);
        }

        public static void Error(LogContextEnum context, Exception exception)
        {
            Error(context, exception?.Message, exception);
        }

        public static void Error(LogContextEnum context, string message, Exception exception)
        {
            Write(LogTypeEnum.Error, context, message, exception.GetLoggableObject());
        }

        public static void Write(LogTypeEnum type, LogContextEnum context, string message, object additionalData)
        {
            // null is possible in design-time - do not invoke writer in order to prevent XAML preview errors
            _logWriter?.Invoke(type, context, message, additionalData);
        }

        private static void DefaultLogWriter(LogTypeEnum type, LogContextEnum context, string message, object additionalData)
        {
            if (_logger == null)
            {
                throw new InvalidOperationException($"Logging is not initialized. Run '{nameof(Initialize)}' first.");
            }

            // does not block invoking code
            Task.Run(async () =>
                {
                    try
                    {
                        var logRecord = new LogRecord()
                            {
                                UniqueId = $"{Guid.NewGuid():N}{Guid.NewGuid():N}", // double GUID
                                KioskVersion = _kioskVersion,
                                LocalTime = DateTime.Now,
                                Type = type,
                                Context = context,
                                Message = message,
                                AdditionalDataJson = _logger.SerializeObject(additionalData),
                            };

                        await _logger.MoveLogRecordToQueueAsync(logRecord);
                    }
                    catch
                    {
                        // ignore to avoid infinite recursion
                    }
                });
        }

        private static LogWriteDelegate _logWriter;

        private static LoggerBase _logger;

        private static string _kioskVersion;

        public static void Initialize(LoggerBase logger, string kioskVersion)
        {
            Assure.ArgumentNotNull(logger, nameof(logger));
            Assure.ArgumentNotNull(kioskVersion, nameof(kioskVersion));

            _logger = logger;
            _logWriter = logger.LogWriter ?? DefaultLogWriter;
            _kioskVersion = kioskVersion;
        }
    }
}