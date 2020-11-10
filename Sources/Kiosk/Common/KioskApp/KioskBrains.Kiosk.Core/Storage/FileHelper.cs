using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskBrains.Kiosk.Core.Storage
{
    public static class FileHelper
    {
        /// <summary>
        /// Safe operation.
        /// </summary>
        public static async Task WriteFileViaTempAsync(
            StorageFolder folder,
            string filename,
            string fileContent)
        {
            Assure.ArgumentNotNull(folder, nameof(folder));
            Assure.ArgumentNotNull(filename, nameof(filename));

            await ThreadHelper.RunInNewThreadAsync(async () =>
                {
                    var tryNumber = 1;
                    Retry:
                    try
                    {
                        // .temp in order to prevent reading before write completion
                        var file = await folder.CreateFileAsync(filename + ".temp", CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteTextAsync(file, fileContent, UnicodeEncoding.Utf8);

                        // rename ready file
                        await file.RenameAsync(filename, NameCollisionOption.ReplaceExisting);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(LogContextEnum.File, $"Writing '{filename}' via temp failed (try {tryNumber}).", ex);

                        tryNumber++;
                        if (tryNumber <= 3)
                        {
                            goto Retry;
                        }

                        throw;
                    }
                });
        }
    }
}