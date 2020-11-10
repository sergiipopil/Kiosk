using KioskBrains.Common.KioskState;

namespace KioskBrains.Common.Api
{
    public class KioskSyncPostRequest
    {
        public KioskMonitorableState KioskState { get; set; }

        /// <summary>
        /// Passed via JSON to avoid double conversion on kiosk side (since log is saved in JSON locally).
        /// </summary>
        public string LogRecordsJson { get; set; }

        /// <summary>
        /// Passed via JSON to avoid double conversion on kiosk side (since transactions are saved in JSON locally).
        /// </summary>
        public string TransactionContainersJson { get; set; }
    }
}