using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Frends.Radon
{
    public interface IEventReader
    {
        IEnumerable<LogEvent> ReadEvents();
    }

    public class EventReader : IEventReader
    {
        private readonly EventLog _log;
        private readonly TimeSpan _timeLimit;

        public EventReader(EventLog log, TimeSpan timeLimit)
        {
            _log = log;
            _timeLimit = timeLimit;
        }

        public IEnumerable<LogEvent> ReadEvents()
        {
            return GetLogEvents();
        }

        private IEnumerable<LogEvent> GetLogEvents()
        {
            IEnumerable<EventLogEntry> entries = null;
            var counter = 0;

            do
            {
                try
                {
                    entries = GetEventLogEntries();
                }
                catch (ArgumentException e)
                {
                    // ArgumentException - out of bounds is thrown if some events are cleanup during getting log events
                    if (!e.Message.Contains("out of bounds") || counter > 2)
                    {
                        throw;
                    }
                    // Tries to get events for 3 times before throwing
                    counter++;
                    Trace.TraceError("Out of bounds occured (maybe events were cleaned up during get), retrying to get log events: "+e);
                }
            } while (entries == null);

            return entries.Select(GetLogEvent);
        }

        private IEnumerable<EventLogEntry> GetEventLogEntries()
        {
            var result = new List<EventLogEntry>();
            var entries = _log.Entries;

            for (var i = entries.Count - 1; i >= 0; i--)
            {
                var logEvent = entries[i];
                if (logEvent.TimeGenerated > DateTime.Now - _timeLimit)
                {
                    result.Add(entries[i]);
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        private static LogEvent GetLogEvent(EventLogEntry entry)
        {
            return new LogEvent
            {
                EntryType = entry.EntryType,
                TimeGenerated = entry.TimeGenerated,
                Source = entry.Source,
                Message = entry.Message,
                Category = entry.Category,
                CategoryNumber = entry.CategoryNumber,
                InstanceID = entry.InstanceId,
                EventID = (short)entry.EventID, // cast to short so BizTalk events also get the same ID as in the event log (RN-48)
                Index = entry.Index,
                ErrorLevel = GetErrorLevel(entry.EntryType)
            };
        }

        private static int GetErrorLevel(EventLogEntryType type)
        {
            switch(type)
            {
                case EventLogEntryType.FailureAudit: return 4;
                case EventLogEntryType.SuccessAudit: return 3;
                case EventLogEntryType.Error: return 2;
                case EventLogEntryType.Warning: return 1;
                case EventLogEntryType.Information: return 0;
                default: return 0;
            }
        }
    }
}