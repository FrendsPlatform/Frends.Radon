using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace Frends.Radon
{
    public class ExternalEventsReader : IEventReader
    {
        private readonly IFilterConfiguration _filterConfig;

        public ExternalEventsReader(IFilterConfiguration filterConfig)
        {
            _filterConfig = filterConfig;
        }

        public IEnumerable<LogEvent> ReadEvents()
        {
            if (string.IsNullOrWhiteSpace(_filterConfig.ExternalEventsXml))
            {
                return new List<LogEvent>();
            }

            var root = XElement.Parse(_filterConfig.ExternalEventsXml);
            var events = root.Elements("Event").Select(eventXml =>
            {
                var category = (string)GetElementOrDefault<string>(eventXml.Element("Category"));
                var categoryNumber = (short) GetElementOrDefault<int>(eventXml.Element("CategoryNumber"));
                var entryType = (EventLogEntryType)Enum.Parse(typeof(EventLogEntryType), (string)GetElementOrDefault<EventLogEntryType>(eventXml.Element("EntryType")));
                var errorLevel = (int)GetElementOrDefault<int>(eventXml.Element("ErrorLevel"));
                var eventId = (short) GetElementOrDefault<int>(eventXml.Element("EventID"));
                var index = (int)GetElementOrDefault<int>(eventXml.Element("Index"));
                var instanceId = (long)GetElementOrDefault<long>(eventXml.Element("InstanceID"));
                var message = (string)GetElementOrDefault<string>(eventXml.Element("Message"));
                var source = (string)GetElementOrDefault<string>(eventXml.Element("Source"));
                var timeGenerated = (DateTime)GetElementOrDefault<DateTime>(eventXml.Element("TimeGenerated"));
                
                return new LogEvent
                {
                    Category = category,
                    CategoryNumber = categoryNumber,
                    EntryType = entryType,
                    ErrorLevel = errorLevel,
                    EventID = eventId,
                    Index = index,
                    InstanceID = instanceId,
                    Message = message,
                    Source = source,
                    TimeGenerated = timeGenerated
                };
            });

            return events.OrderByDescending(e => e.TimeGenerated);
        }

        private XElement GetElementOrDefault<T>(XElement element)
        {
            if (element == null)
            {
                return new XElement("temp", default(T));
            }

            return element;
        }
    }
}