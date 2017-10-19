using System;
using System.Collections.Generic;
using System.Linq;

namespace Frends.Radon
{
    public class LogEventFilter : IEventReader
    {
        private readonly IEventReader _reader;
        private readonly IFilterConfiguration _config;


        public LogEventFilter(IEventReader reader, IFilterConfiguration config)
        {
            _reader = reader;
            _config = config;
        }

        public IEnumerable<LogEvent> ReadEvents()
        {
            var unfilteredEvents = _reader.ReadEvents();

            IEnumerable<LogEvent> query;
            if (String.IsNullOrEmpty(_config.FilterString))
            {
                query = unfilteredEvents; // no filter defined
            }
            else
            {
                try
                {
                    query = unfilteredEvents.AsQueryable().Where(_config.FilterString);
                }
                catch (ParseException pe)
                {
                    // add some extra info to the error message
                    throw new ArgumentException(String.Format("Could not parse filter string '{0}': {1}", _config.FilterString, pe), pe);
                }
            }
            IEnumerable<LogEvent> result;
            if (_config.UseMaxMessages)
            {
                result = query.Take(_config.MaxMessages);
            }
            else
            {
                result = query;
            }
            var count = result.Count();
            if (count > 0)
            {
                Console.WriteLine("Events found: " + count);
            }
            else
            {
                Console.WriteLine("No events were found for FRENDS Radon report.");
            }
            return result;
        }
    }
}