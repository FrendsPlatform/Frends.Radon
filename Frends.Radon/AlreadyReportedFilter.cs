using System.Collections.Generic;
using System.Linq;

namespace Frends.Radon
{
    public class AlreadyReportedFilter : IEventReader
    {
        private readonly IEventReader _eventReader;
        private readonly IEventIdentificationStore _identificationStore;

        public AlreadyReportedFilter(IEventReader eventReader, IEventIdentificationStore identificationStore)
        {
            _eventReader = eventReader;
            _identificationStore = identificationStore;
        }

        public IEnumerable<LogEvent> ReadEvents()
        {
            var oldEventIdentification = _identificationStore.GetAlreadyReportedEventIdentification();
            var events = _eventReader.ReadEvents();
            
            if (oldEventIdentification == null)
            {
                return events;
            }

            var filteredEvents = 
                events.TakeWhile(
                    e =>
                    {
                        var newerThan = e.TimeGenerated.ToUniversalTime() > oldEventIdentification.TimeStampUtc;

                        if (newerThan)
                        {
                            // The event is newer than the old event
                            return true;
                        }
                        else
                        {
                            if (e.TimeGenerated.ToUniversalTime() == oldEventIdentification.TimeStampUtc)
                            {
                                var eventHash = HashBuilder.BuildEventIdentification(e).Hash;
                                if (eventHash != oldEventIdentification.Hash)
                                {
                                    // The event is the same age as the old event but has a different hash
                                    return true;
                                }
                            }
                        }
                        return false; // The event wasn't newer than than the old event or the same age and hash -> stop returning older events
                    });

            return filteredEvents;
        }
    }
}
