using System.Collections.Generic;
using System.Linq;

namespace Frends.Radon
{
    public interface IRadonExecutor
    {
        IList<LogEvent> GetLogEvents();
        void SendReport(IEnumerable<LogEvent> events);
        void SaveAlreadyReportedEventIdentification(LogEvent logEvent);

        IEmailConfiguration EmailConfig { get; }
    }

    public class RadonExecutor : IRadonExecutor
    {
        public IEmailConfiguration EmailConfig { get; private set; }

        private readonly IFilterConfiguration _filterConfig;

        private readonly IEventIdentificationStore _eventIdentificationStore;

        private readonly IEventReader _eventReader;

        internal RadonExecutor(IEmailConfiguration emailConfig, IFilterConfiguration filterConfig, IEventIdentificationStore eventIdentificationStore, IEventReader eventReader)
        {
            EmailConfig = emailConfig;
            _filterConfig = filterConfig;
            _eventIdentificationStore = eventIdentificationStore;
            _eventReader = eventReader;
        }

        public RadonExecutor(IRadonConfigurationManager manager)
            : this(emailConfig: manager.GetEmailConfig(),
                   filterConfig: manager.GetFilterConfig(),
                   eventIdentificationStore: new EventIdentificationStore(manager.GetFilterConfig()),
                   eventReader: GetAlreadyReportedFilter(manager.GetFilterConfig()))
        {
        }

        public IList<LogEvent> GetLogEvents()
        {
            return _eventReader.ReadEvents().ToList();
        }

        public void SendReport(IEnumerable<LogEvent> events)
        {
            using (var reportSender = this.EmailConfig.GetReportSender())
            {
                var htmlReportGenerator = new HtmlReportGenerator(events, this._filterConfig, this.EmailConfig);

                reportSender.SendReport(htmlReportGenerator.CreateHtmlReport(), htmlReportGenerator.CreateSubject());
            }
        }

        public void SaveAlreadyReportedEventIdentification(LogEvent logEvent)
        {
            this._eventIdentificationStore.SaveAlreadyReportedEventIdentification(logEvent);
        }

        private static IEventReader GetAlreadyReportedFilter(IFilterConfiguration filterConfig)
        {
            var eventReader = GetEventSource(filterConfig);
            var logEventFilter = new LogEventFilter(eventReader, filterConfig);

            return new AlreadyReportedFilter(logEventFilter, new EventIdentificationStore(filterConfig));
        }

        private static IEventReader GetEventSource(IFilterConfiguration filterConfig)
        {
            switch (filterConfig.EventSource)
            {
                case EventSource.External:
                    return new ExternalEventsReader(filterConfig); // TODO: Should we filter the events by the time limit as with the event log?
                default:
                case EventSource.EventLog:
                    var eventLog = new EventLogFactory().CreateEventLog(filterConfig);
                    var eventReader = new EventReader(eventLog, filterConfig.TimeLimit);
                    return eventReader;
            }

        }
    }
}
