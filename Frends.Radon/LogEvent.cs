using System;
using System.Diagnostics;

namespace Frends.Radon
{
    public class LogEvent
    {
        public short CategoryNumber { get;  set; }

        public string Category { get; set; }

        public string Message { get; set; }

        public string Source { get; set; }

        public DateTime TimeGenerated { get; set; }

        public EventLogEntryType EntryType { get; set; }

        public long InstanceID { get; set; }

        public int Index { get; set; }

        public short EventID { get; set; }

        public int ErrorLevel { get; set; }
    }
}