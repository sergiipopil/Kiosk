using System.IO;
using System.Threading.Tasks;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.CommonNet.Helpers;
using Newtonsoft.Json;

namespace KioskBrains.Kiosk.CommonNet.Logging
{
    public class Logger : LoggerBase
    {
        private readonly string _logFolderPath;

        public Logger()
        {
            _logFolderPath = Path.Combine(KioskFolderNames.KioskLocation, KioskFolderNames.Kiosk, KioskFolderNames.Logs);
        }

        public override string SerializeObject(object data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public override Task MoveLogRecordToQueueAsync(LogRecord logRecord)
        {
            Assure.ArgumentNotNull(logRecord, nameof(logRecord));
            var filename = $"{logRecord.UniqueId}.json";
            var filePath = Path.Combine(_logFolderPath, filename);
            var fileContent = SerializeObject(logRecord);
            return FileHelper.SaveFileAsync(filePath, fileContent);
        }
    }
}