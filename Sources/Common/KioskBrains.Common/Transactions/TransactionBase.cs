using System;

namespace KioskBrains.Common.Transactions
{
    public abstract class TransactionBase
    {
        public string UniqueId { get; set; }

        public abstract TransactionWorkflowEnum Workflow { get; }

        public TransactionStatusEnum Status { get; set; }

        public DateTime LocalStartedOn { get; set; }

        public DateTime LocalEndedOn { get; set; }

        public TransactionCompletionStatusEnum? CompletionStatus { get; set; }
    }
}