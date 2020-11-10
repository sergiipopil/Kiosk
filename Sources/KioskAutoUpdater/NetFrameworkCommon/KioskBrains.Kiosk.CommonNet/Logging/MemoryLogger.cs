using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KioskBrains.Common.Logging;
using Newtonsoft.Json;

namespace KioskBrains.Kiosk.CommonNet.Logging
{
    public class MemoryLogger : LoggerBase
    {
        public List<string> LogRecords { get; } = new List<string>();

        public MemoryLogger()
        {
            LogWriter = (type, context, message, data) =>
                {
                    var messageBuilder = new StringBuilder();
                    messageBuilder.Append(type);
                    messageBuilder.Append(": ");
                    messageBuilder.Append(message);
                    if (data != null)
                    {
                        messageBuilder.Append(" - ");
                        messageBuilder.Append(JsonConvert.SerializeObject(data));
                    }
                    LogRecords.Add(messageBuilder.ToString());
                };
        }

        public override LogWriteDelegate LogWriter { get; }

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