namespace KioskBrains.Common.Transactions
{
    public enum TransactionStatusEnum
    {
        Unspecified = 0,
        Completed = 1,
        CancelledByUser = 2,
        CancelledByTimeout = 3,
        CancelledByError = 4,
        CancelledByExit = 5,
    }
}