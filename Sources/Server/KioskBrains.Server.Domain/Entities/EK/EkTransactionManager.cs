using System;
using System.Threading.Tasks;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;
using KioskBrains.Common.Transactions;
using Newtonsoft.Json;

namespace KioskBrains.Server.Domain.Entities.EK { 
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

            transaction.Status = TransactionStatusEnum.Completed;
            transaction.LocalEndedOn = DateTime.Now;
            return transaction;
        }        
    }
}