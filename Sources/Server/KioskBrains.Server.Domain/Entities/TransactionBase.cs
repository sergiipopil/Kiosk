using System;
using System.ComponentModel.DataAnnotations;
using KioskBrains.Common.Transactions;
using KioskBrains.Server.Domain.Entities.Common;
using ApiTransactionBase = KioskBrains.Common.Transactions.TransactionBase;

namespace KioskBrains.Server.Domain.Entities
{
    public abstract class TransactionBase : EntityBase
    {
        public int KioskId { get; set; }

        public Kiosk Kiosk { get; set; }

        [Required]
        [StringLength(255)]
        public string UniqueId { get; set; }

        public DateTime AddedOnUtc { get; set; }

        public TransactionStatusEnum Status { get; set; }

        public DateTime LocalStartedOn { get; set; }

        public DateTime LocalEndedOn { get; set; }

        public TransactionCompletionStatusEnum? CompletionStatus { get; set; }

        protected void ApplyBaseApiModel(int kioskId, DateTime utcNow, ApiTransactionBase apiModel)
        {
            KioskId = kioskId;
            AddedOnUtc = utcNow;
            UniqueId = apiModel.UniqueId;
            Status = apiModel.Status;
            LocalStartedOn = apiModel.LocalStartedOn;
            LocalEndedOn = apiModel.LocalEndedOn;
            CompletionStatus = apiModel.CompletionStatus;
        }
    }
}