using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using KioskBrains.Common.Contracts;
using KioskBrains.Server.Common.Log;
using KioskBrains.Server.Common.Services;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Entities.DbStorage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskBrains.Server.Domain.Managers.Integration
{
    /// <summary>
    /// Should be added to DI as Transient.
    /// All methods are safe.
    /// </summary>
    public class IntegrationLogManager : IIntegrationLogManager
    {
        private readonly KioskBrainsContext _dbContext;

        private readonly ILogger<IntegrationLogRecord> _logger;

        public IntegrationLogManager(
            KioskBrainsContext dbContext,
            ILogger<IntegrationLogRecord> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        private readonly IntegrationLogRecord _logRecord = new IntegrationLogRecord();

        private readonly StringBuilder _requestLogBuilder = new StringBuilder();

        private readonly StringBuilder _responseLogBuilder = new StringBuilder();

        private Stopwatch _stopwatch;

        public void StartNewLogRecord(
            string externalSystem,
            IntegrationRequestDirectionEnum direction)
        {
            try
            {
                Assure.ArgumentNotNull(externalSystem, nameof(externalSystem));

                _stopwatch = Stopwatch.StartNew();

                _logRecord.Direction = direction;
                _logRecord.ExternalSystem = externalSystem;
                _logRecord.RequestedOnUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.UnhandledException, nameof(StartNewLogRecord), ex);
            }
        }

        public void LogToRequest(
            string key,
            object requestObjectOrStringOrException)
        {
            try
            {
                string requestValue;
                if (requestObjectOrStringOrException is string requestString)
                {
                    requestValue = requestString;
                }
                else if (requestObjectOrStringOrException is Exception requestException)
                {
                    requestValue = requestException.ToString();
                }
                else
                {
                    try
                    {
                        requestValue = JsonConvert.SerializeObject(requestObjectOrStringOrException);
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = $"Serialization of {nameof(requestObjectOrStringOrException)} ({requestObjectOrStringOrException?.GetType()}) to JSON failed: {ex}";
                        requestValue = errorMessage;
                        _logger.LogError(LoggingEvents.UnhandledException, errorMessage);
                    }
                }

                _requestLogBuilder.AppendLine($"{key}: {requestValue}");
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.UnhandledException, nameof(LogToRequest), ex);
            }
        }

        public void LogToResponse(
            string key,
            object responseObjectOrStringOrException)
        {
            try
            {
                string responseValue;
                if (responseObjectOrStringOrException is string responseString)
                {
                    responseValue = responseString;
                }
                else if (responseObjectOrStringOrException is Exception responseException)
                {
                    responseValue = responseException.ToString();
                }
                else
                {
                    try
                    {
                        responseValue = JsonConvert.SerializeObject(responseObjectOrStringOrException);
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = $"Serialization of {nameof(responseObjectOrStringOrException)} ({responseObjectOrStringOrException?.GetType()}) to JSON failed: {ex}";
                        responseValue = errorMessage;
                        _logger.LogError(LoggingEvents.UnhandledException, errorMessage);
                    }
                }

                _responseLogBuilder.AppendLine($"{key}: {responseValue}");
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.UnhandledException, nameof(LogToResponse), ex);
            }
        }

        public async Task CompleteLogRecordAsync()
        {
            try
            {
                _stopwatch.Stop();
                _logRecord.ProcessingTime = _stopwatch.Elapsed;

                _logRecord.Request = _requestLogBuilder.ToString();
                _logRecord.Response = _responseLogBuilder.ToString();

                _dbContext.IntegrationLogRecords.Add(_logRecord);

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.UnhandledException, nameof(CompleteLogRecordAsync), ex);
            }
        }
    }
}