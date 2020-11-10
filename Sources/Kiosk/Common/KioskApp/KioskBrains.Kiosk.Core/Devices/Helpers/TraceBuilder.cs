using System;
using System.Collections.Generic;
using System.Linq;
using KioskBrains.Common.Logging;

namespace KioskBrains.Kiosk.Core.Devices.Helpers
{
    public class TraceBuilder : List<string>, ILoggableObject
    {
        private DateTime _startedOn = DateTime.Now;

        public void AddTrace(string message)
        {
            Add($"{DateTime.Now:HH:mm:ss}: {message}");
        }

        public bool IsEmpty => Count == 0;

        internal void Reset()
        {
            _startedOn = DateTime.Now;
            this.Clear();
        }

        public object GetLogObject()
        {
            var trace = this.ToList(); // copy

            // add start/end
            trace.Insert(0, $"{_startedOn:yyyy-MM-dd HH:mm:ss}: Started.");
            trace.Add($"{DateTime.Now:HH:mm:ss}: Ended.");

            return trace.ToArray();
        }
    }
}