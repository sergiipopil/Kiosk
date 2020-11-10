using System.Threading.Tasks;

namespace KioskBrains.Server.Common.Services
{
    /// <summary>
    /// All method implementations should be safe.
    /// </summary>
    public interface IIntegrationLogManager
    {
        void StartNewLogRecord(string externalSystem, IntegrationRequestDirectionEnum direction);

        void LogToRequest(string key, object requestObjectOrStringOrException);

        void LogToResponse(string key, object responseObjectOrStringOrException);

        Task CompleteLogRecordAsync();
    }
}