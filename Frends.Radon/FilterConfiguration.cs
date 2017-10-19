using System;

namespace Frends.Radon
{
    public interface IFilterConfiguration
    {
        string FilterString { get; }

        TimeSpan TimeLimit { get; }

        int MaxMessages { get; }

        bool UseMaxMessages { get; }

        string RemoteMachine { get; }

        string EventLogName { get; }

        EventSource EventSource { get; }

        string ExternalEventsXml { get; }
    }

    public enum EventSource
    {
        EventLog,
        External
    }

    public class FilterConfiguration : IFilterConfiguration
    {
        public string FilterString { get; private set; }

        public TimeSpan TimeLimit { get; private set; }

        public int MaxMessages { get; private set; }

        public bool UseMaxMessages { get; private set; }

        public string RemoteMachine { get; private set; }

        public string EventLogName { get; private set; }

        public EventSource EventSource { get; private set; }

        public string ExternalEventsXml { get; private set; }

        public FilterConfiguration(string filterString, 
                                   TimeSpan timeLimit, 
                                   int maxMessages, 
                                   string remoteMachine,
                                   string eventLogName,
                                   EventSource eventSource = EventSource.EventLog,
                                   string externalEventsXml = "")
        {
            FilterString = filterString ?? string.Empty;
            TimeLimit = timeLimit;
            MaxMessages = maxMessages;
            UseMaxMessages = MaxMessages > 0;
            RemoteMachine = Parser.TrimLineOrReturnStringEmptyIfNull(remoteMachine);
            EventLogName = Parser.TrimLineOrReturnStringEmptyIfNull(eventLogName);
            EventSource =  eventSource;
            ExternalEventsXml = externalEventsXml;
        }
    }
}