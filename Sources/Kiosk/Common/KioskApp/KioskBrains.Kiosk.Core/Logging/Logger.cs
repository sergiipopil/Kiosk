using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Storage;
using Newtonsoft.Json;

namespace KioskBrains.Kiosk.Core.Logging
{
    public class Logger : LoggerBase
    {
        public override string SerializeObject(object data)
        {
            return JsonConvert.SerializeObject(data);
        }

        private StorageFolder _logFolder;

        public override async Task MoveLogRecordToQueueAsync(LogRecord logRecord)
        {
            Assure.ArgumentNotNull(logRecord, nameof(logRecord));

            // sync problems are acceptable
            if (_logFolder == null)
            {
                _logFolder = await GetLogFolderAsync();
            }

            var filename = $"{logRecord.UniqueId}.json";
            var fileContent = SerializeObject(logRecord);

            // .temp in order to prevent reading before write completion
            var file = await _logFolder.CreateFileAsync(filename + ".temp", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, fileContent, UnicodeEncoding.Utf8);

            // rename ready file
            await file.RenameAsync(filename, NameCollisionOption.ReplaceExisting);
        }

        public static async Task<StorageFolder> GetLogFolderAsync()
        {
            var kioskFolder = await StorageHelper.GetKioskRootFolderAsync();
            return await kioskFolder.GetFolderAsync(KioskFolderNames.Logs);
        }
    }
}