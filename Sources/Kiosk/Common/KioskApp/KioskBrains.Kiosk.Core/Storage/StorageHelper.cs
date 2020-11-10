using System;
using System.Threading.Tasks;
using Windows.Storage;
using KioskBrains.Common.Constants;

namespace KioskBrains.Kiosk.Core.Storage
{
    public static class StorageHelper
    {
        public const string DateModifiedPropertyName = "System.DateModified";

        public static async Task<StorageFolder> GetKioskRootFolderAsync()
        {
            return await KnownFolders.PicturesLibrary.GetFolderAsync(KioskFolderNames.Kiosk);
        }

        public static async Task<StorageFolder> GetTempFolderAsync()
        {
            var kioskFolder = await GetKioskRootFolderAsync();
            return await kioskFolder.CreateFolderAsync(KioskFolderNames.Temp, CreationCollisionOption.OpenIfExists);
        }
    }
}