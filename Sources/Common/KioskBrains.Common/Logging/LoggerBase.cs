using System.Threading.Tasks;

namespace KioskBrains.Common.Logging
{
    /// <summary>
    /// All methods are invoked in separate thread.
    /// No need to process exceptions as well.
    /// </summary>
    public abstract class LoggerBase
    {
        public abstract string SerializeObject(object data);

        public abstract Task MoveLogRecordToQueueAsync(LogRecord logRecord);

        /// <summary>
        /// Completely overrides log write function (in this case no need to implement other methods).
        /// </summary>
        public virtual LogWriteDelegate LogWriter => null;
    }
}