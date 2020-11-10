using System;
using System.Threading.Tasks;
using Windows.Storage;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;
using KioskBrains.Common.Transactions;
using KioskBrains.Kiosk.Core.Components;
using KioskBrains.Kiosk.Core.Components.Operations;
using KioskBrains.Kiosk.Core.ServerSync;
using KioskBrains.Kiosk.Core.Storage;
using KioskBrains.Kiosk.Helpers.Threads;
using Newtonsoft.Json;

namespace KioskBrains.Kiosk.Core.Transactions
{
    public class TransactionManager
    {
        #region Singleton

        public static TransactionManager Current { get; } = new TransactionManager();

        private TransactionManager()
        {
        }

        #endregion

        public TTransaction StartNewTransaction<TTransaction>()
            where TTransaction : TransactionBase, new()
        {
            var transaction = Activator.CreateInstance<TTransaction>();
            transaction.UniqueId = $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
            transaction.LocalStartedOn = DateTime.Now;
            return transaction;
        }

        public void EndTransaction(
            TransactionBase transaction,
            TransactionStatusEnum status)
        {
            Assure.ArgumentNotNull(transaction, nameof(transaction));

            transaction.Status = status;
            transaction.LocalEndedOn = DateTime.Now;

            SaveTransactionForSendingAsync(transaction);
        }

        private StorageFolder _transactionFolder;

        private Task SaveTransactionForSendingAsync(TransactionBase transaction)
        {
            Assure.ArgumentNotNull(transaction, nameof(transaction));

            return ThreadHelper.RunInNewThreadAsync(async () =>
                {
                    try
                    {
                        // sync issues with race conditions are acceptable
                        if (_transactionFolder == null)
                        {
                            _transactionFolder = await GetTransactionFolderAsync();
                        }

                        var transactionContainer = new TransactionContainer()
                            {
                                Workflow = transaction.Workflow,
                                TransactionJson = JsonConvert.SerializeObject(transaction)
                            };

                        var filename = $"{transaction.UniqueId}.json";
                        var fileContent = JsonConvert.SerializeObject(transactionContainer);

                        await FileHelper.WriteFileViaTempAsync(_transactionFolder, filename, fileContent);

                        // send completed and failed transactions as soon as possible
                        if (transaction.Status == TransactionStatusEnum.Completed
                            || transaction.Status == TransactionStatusEnum.CancelledByError)
                        {
                            var syncService = ComponentManager.Current.GetComponent<IServerSyncServiceContract>(mandatory: false);
                            syncService?.ScheduleImmediateSync.InvokeAsync(new EmptyOperationRequest());
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(LogContextEnum.Workflow, nameof(SaveTransactionForSendingAsync), ex);
                    }
                });
        }

        public static async Task<StorageFolder> GetTransactionFolderAsync()
        {
            var kioskFolder = await StorageHelper.GetKioskRootFolderAsync();
            return await kioskFolder.GetFolderAsync(KioskFolderNames.Transactions);
        }
    }
}