using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Helpers.Threads;
using Newtonsoft.Json;

namespace KioskBrains.Kiosk.Core.Storage
{
    // JSON serialization is used.
    public class AppDataStorage
    {
        #region Singleton

        public static AppDataStorage Current { get; } = new AppDataStorage();

        private AppDataStorage()
        {
        }

        #endregion

        private const string FileExtension = ".json";

        private StorageFolder _storageFolder;

        private async Task<StorageFolder> CreateOrOpenFolderAsync(string folderName, CancellationToken cancellationToken)
        {
            // no sync but it's acceptable
            if (_storageFolder == null)
            {
                var kioskFolder = await StorageHelper.GetKioskRootFolderAsync();
                _storageFolder = await kioskFolder.CreateFolderAsync(KioskFolderNames.AppData, CreationCollisionOption.OpenIfExists);
            }

            var folder = await _storageFolder
                .CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists)
                .AsTask(cancellationToken);
            return folder;
        }

        public Task SaveRecordAsync<TRecordContent>(
            string folderName,
            string recordName,
            TRecordContent recordContent,
            CancellationToken cancellationToken
        )
            where TRecordContent : class, new()
        {
            Assure.ArgumentNotNull(folderName, nameof(folderName));
            Assure.ArgumentNotNull(recordName, nameof(recordName));

            return ThreadHelper.RunInBackgroundThreadAsync(async () =>
                {
                    var fileContent = JsonConvert.SerializeObject(recordContent);

                    var storageFolder = await CreateOrOpenFolderAsync(folderName, cancellationToken);
                    var fileName = recordName + FileExtension;
                    var storageFile = await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists)
                        .AsTask(cancellationToken);
                    await FileIO.WriteTextAsync(storageFile, fileContent)
                        .AsTask(cancellationToken);
                });
        }

        public Task<TRecordContent> LoadRecordAsync<TRecordContent>(
            string folderName,
            string recordName,
            bool failIfNotFound,
            CancellationToken cancellationToken
        )
            where TRecordContent : class, new()
        {
            Assure.ArgumentNotNull(folderName, nameof(folderName));
            Assure.ArgumentNotNull(recordName, nameof(recordName));

            return ThreadHelper.RunInBackgroundThreadAsync(async () =>
                {
                    var storageFolder = await CreateOrOpenFolderAsync(folderName, cancellationToken);
                    var fileName = recordName + FileExtension;
                    StorageFile storageFile;
                    try
                    {
                        storageFile = await storageFolder.GetFileAsync(fileName)
                            .AsTask(cancellationToken);
                    }
                    catch (FileNotFoundException)
                    {
                        if (failIfNotFound)
                        {
                            throw new RecordFileNotFoundException(folderName, fileName);
                        }
                        else
                        {
                            return (TRecordContent)null;
                        }
                    }
                    var fileContent = await FileIO.ReadTextAsync(storageFile)
                        .AsTask(cancellationToken);

                    var recordContent = JsonConvert.DeserializeObject<TRecordContent>(fileContent);

                    return recordContent;
                });
        }

        public Task DeleteRecordAsync(
            string folderName,
            string recordName,
            CancellationToken cancellationToken,
            bool logErrorIfNotFound = true)
        {
            Assure.ArgumentNotNull(folderName, nameof(folderName));
            Assure.ArgumentNotNull(recordName, nameof(recordName));

            return ThreadHelper.RunInBackgroundThreadAsync(async () =>
                {
                    var storageFolder = await CreateOrOpenFolderAsync(folderName, cancellationToken);
                    var fileName = recordName + FileExtension;
                    StorageFile storageFile;
                    try
                    {
                        storageFile = await storageFolder.GetFileAsync(fileName)
                            .AsTask(cancellationToken);
                    }
                    catch (FileNotFoundException)
                    {
                        if (logErrorIfNotFound)
                        {
                            Log.Error(LogContextEnum.File, "File for deletion has not been found.", new
                                {
                                    FolderName = folderName,
                                    RecordName = recordName,
                                });
                        }
                        return;
                    }
                    await storageFile.DeleteAsync(StorageDeleteOption.PermanentDelete)
                        .AsTask(cancellationToken);
                });
        }
    }
}