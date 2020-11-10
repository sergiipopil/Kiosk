using System;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Helpers;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Storage;

namespace KioskBrains.Kiosk.Core.Application
{
    public static class KioskAppNotReadyForUpdateSign
    {
        public static async Task SetAsync()
        {
            try
            {
                await AppDataStorage.Current.SaveRecordAsync<EmptyRecord>(
                    KioskFolderNames.AppData_State,
                    KioskFileNames.KioskAppNotReadyForUpdateSignWithoutExtension,
                    null,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Application, $"{nameof(KioskAppNotReadyForUpdateSign)}.{nameof(SetAsync)}", ex);
            }
        }

        public static async Task UnsetAsync()
        {
            try
            {
                await AppDataStorage.Current.DeleteRecordAsync(
                    KioskFolderNames.AppData_State,
                    KioskFileNames.KioskAppNotReadyForUpdateSignWithoutExtension,
                    CancellationToken.None,
                    logErrorIfNotFound: false);
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Application, $"{nameof(KioskAppNotReadyForUpdateSign)}.{nameof(UnsetAsync)}", ex);
            }
        }
    }
}