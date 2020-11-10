namespace KioskBrains.Common.Transactions
{
    public class TransactionContainer
    {
        public TransactionWorkflowEnum Workflow { get; set; }

        /// <summary>
        /// Passed via JSON since transaction is specific for each workflow.
        /// </summary>
        public string TransactionJson { get; set; }
    }
}