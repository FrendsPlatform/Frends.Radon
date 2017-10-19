using System;
using DotLiquid;

namespace Frends.Radon.TemplateDrops
{
    public class LogEventDrop : Drop
    {
        private readonly LogEvent _logEvent;

        public LogEventDrop(LogEvent logEvent)
        {
            _logEvent = logEvent;
        }

        public short CategoryNumber { get { return _logEvent.CategoryNumber; } }

        public string Category { get { return _logEvent.Category; } }

        public string Message { get { return _logEvent.Message; } }

        public string Source { get { return _logEvent.Source; } }

        public DateTime TimeGenerated { get { return _logEvent.TimeGenerated; } }

        public string EntryType { get { return _logEvent.EntryType.ToString(); } }

        public long InstanceID { get { return _logEvent.InstanceID; } }

        public int Index { get { return _logEvent.Index; } }

        public short EventID { get { return _logEvent.EventID; } }

        public int ErrorLevel { get { return _logEvent.ErrorLevel; } }
    }
}
