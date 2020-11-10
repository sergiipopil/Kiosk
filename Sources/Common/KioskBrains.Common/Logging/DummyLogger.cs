using System;
using System.Threading.Tasks;

namespace KioskBrains.Common.Logging
{
    public class DummyLogger : LoggerBase
    {
        public override LogWriteDelegate LogWriter { get; } = (type, context, message, data) => { };

        public override string SerializeObject(object data)
        {
            throw new NotImplementedException();
        }

        public override Task MoveLogRecordToQueueAsync(LogRecord logRecord)
        {
            throw new NotImplementedException();
        }
    }
}