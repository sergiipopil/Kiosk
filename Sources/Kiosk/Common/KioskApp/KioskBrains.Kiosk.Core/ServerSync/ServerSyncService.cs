using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using KioskBrains.Common.Api;
using KioskBrains.Common.KioskState;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Components;
using KioskBrains.Kiosk.Core.Components.Initialization;
using KioskBrains.Kiosk.Core.Components.Operations;
using KioskBrains.Kiosk.Core.Logging;
using KioskBrains.Kiosk.Core.ServerApi;
using KioskBrains.Kiosk.Core.Transactions;

namespace KioskBrains.Kiosk.Core.ServerSync
{
    public class ServerSyncService : ComponentBase, IServerSyncServiceContract
    {
        public ServerSyncService()
        {
            SyncWorker = new ComponentOperation<EmptyOperationRequest, BasicOperationResponse>(this, nameof(SyncWorker), SyncWorkerAsync);
            ScheduleImmediateSync = new ComponentOperation<EmptyOperationRequest, BasicOperationResponse>(this, nameof(ScheduleImmediateSync), ScheduleImmediateSyncAsync);
        }

        public override bool IsStateMonitorable => false;

        protected override Type[] GetSupportedContracts()
        {
            return new[]
            {
                typeof(IServerSyncServiceContract),
            };
        }

        private readonly TimeSpan StartDelay = TimeSpan.FromSeconds(30);

        private readonly TimeSpan SyncInterval = TimeSpan.FromMinutes(1);

        private readonly TimeSpan NextSyncTimeCheckInterval = TimeSpan.FromSeconds(1);

        private const int MaxFilesPerPackage = 50;

        private bool _isPreviousSyncFailed;

        private StorageFolder _logFolder;

        private StorageFolder _transactionFolder;

        protected override async Task<ComponentInitializeResponse> InitializeAsync(ComponentInitializeRequest request, ComponentOperationContext context)
        {
            _logFolder = await Logger.GetLogFolderAsync();
            _transactionFolder = await TransactionManager.GetTransactionFolderAsync();

#pragma warning disable 4014
            SyncWorker.InvokeAsync(new EmptyOperationRequest());
#pragma warning restore 4014

            Status.SetSelfStatus(ComponentStatusCodeEnum.Ok, null);

            return ComponentInitializeResponse.GetSuccess();
        }

        private ComponentOperation<EmptyOperationRequest, BasicOperationResponse> SyncWorker { get; }

        // ReSharper disable FunctionNeverReturns
        private async Task<BasicOperationResponse> SyncWorkerAsync(EmptyOperationRequest request, ComponentOperationContext context)
        {
            // wait for initialization of app components
            await Task.Delay(StartDelay);

            while (true)
            {
                while (!HasNextSyncTimeCome())
                {
                    await Task.Delay(NextSyncTimeCheckInterval);
                }

                try
                {
                    // todo: check internet presence (via some component)

                    var isAdditionalSyncRequired = await SyncAsync();
                    while (isAdditionalSyncRequired)
                    {
                        isAdditionalSyncRequired = await SyncAsync();
                    }

                    _isPreviousSyncFailed = false;
                }
                catch (Exception ex)
                {
                    // to avoid generation of tons of messages because of internet/algorithm problems
                    if (!_isPreviousSyncFailed)
                    {
                        context.Log.Error(LogContextEnum.Communication, $"'{nameof(ServerSyncService)}' sync iteration failed.", ex);
                    }

                    _isPreviousSyncFailed = true;
                }

                SetNextSyncTime(DateTime.Now + SyncInterval);
            }
        }
        // ReSharper restore FunctionNeverReturns

        /// <returns>Is additional sync required (if there are still a lot of log/transaction records).</returns>
        private async Task<bool> SyncAsync()
        {
            var request = new KioskSyncPostRequest
            {
                KioskState = ComponentManager.Current.GetKioskMonitorableState(),
            };

            var logRecords = await GetSyncJsonFromFolderAsync(_logFolder);
            var transactions = await GetSyncJsonFromFolderAsync(_transactionFolder);

            request.LogRecordsJson = logRecords.Json;
            request.TransactionContainersJson = transactions.Json;

            var response = await ServerApiProxy.Current.SyncAsync(request);

            await DeleteProcessedFilesAsync(
                logRecords.IncludedFiles,
                transactions.IncludedFiles);

            var isAdditionalSyncRequired = logRecords.IsAdditionalSyncRequired
                                           || transactions.IsAdditionalSyncRequired;

            return isAdditionalSyncRequired;
        }

        private async Task<(string Json, StorageFile[] IncludedFiles, bool IsAdditionalSyncRequired)>
            GetSyncJsonFromFolderAsync(StorageFolder folder, int maxFilesPerPackage = MaxFilesPerPackage)
        {
            var folderFiles = (await folder.GetFilesAsync())
                .Where(x => x.Name.EndsWith(".json")) // ignore temp files
                .ToArray();

            var includedFiles = folderFiles
                .OrderBy(x => x.DateCreated)
                .Take(maxFilesPerPackage)
                .ToArray();

            string json = null;
            if (includedFiles.Length > 0)
            {
                var jsonBuilder = new StringBuilder("[");
                for (var i = 0; i < includedFiles.Length; i++)
                {
                    if (i != 0)
                    {
                        jsonBuilder.Append(",");
                    }

                    var fileContent = await FileIO.ReadTextAsync(includedFiles[i]);
                    jsonBuilder.Append(fileContent);
                }

                jsonBuilder.Append("]");
                json = jsonBuilder.ToString();
            }

            // request additional sync if non-included files remain
            var isAdditionalSyncRequired = folderFiles.Length > maxFilesPerPackage;

            return (json, includedFiles, isAdditionalSyncRequired);
        }

        private async Task DeleteProcessedFilesAsync(params StorageFile[][] processedFileGroups)
        {
            foreach (var processedFileGroup in processedFileGroups.Where(x => x?.Length > 0))
            {
                foreach (var processedFile in processedFileGroup)
                {
                    try
                    {
                        await processedFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }
                    catch
                    {
                        // ignore - try to delete other files
                    }
                }
            }
        }

        private DateTime? _nextSyncTime;

        private readonly object _nextSyncTimeLocker = new object();

        private bool HasNextSyncTimeCome()
        {
            lock (_nextSyncTimeLocker)
            {
                if (_nextSyncTime == null)
                {
                    return true;
                }

                if (_nextSyncTime.Value > DateTime.Now)
                {
                    return false;
                }

                // sync time has come
                // clear _nextSyncTime in order to allow setting of new time by SyncWorker at the end of sync or by ScheduleImmediateSync
                _nextSyncTime = null;
                return true;
            }
        }

        private void SetNextSyncTime(DateTime nextSyncTime)
        {
            lock (_nextSyncTimeLocker)
            {
                if (_nextSyncTime == null)
                {
                    _nextSyncTime = nextSyncTime;
                }
                // set only if new sync time is smaller than current one
                else if (_nextSyncTime.Value > nextSyncTime)
                {
                    _nextSyncTime = nextSyncTime;
                }
                else
                {
                    // ignore
                }
            }
        }

        private Task<BasicOperationResponse> ScheduleImmediateSyncAsync(EmptyOperationRequest request, ComponentOperationContext operationContext)
        {
            SetNextSyncTime(DateTime.Now);

            return Task.FromResult(BasicOperationResponse.GetSuccess());
        }

        public ComponentOperation<EmptyOperationRequest, BasicOperationResponse> ScheduleImmediateSync { get; }
    }
}