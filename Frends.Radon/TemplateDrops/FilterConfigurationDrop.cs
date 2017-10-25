using System;
using DotLiquid;

namespace Frends.Radon.TemplateDrops
{
    public class FilterConfigurationDrop : Drop
    {
        private readonly IFilterConfiguration _configuration;

        public FilterConfigurationDrop(IFilterConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string FilterString { get { return _configuration.FilterString; } }

        public TimeSpan TimeLimit { get { return _configuration.TimeLimit; } }

        public int MaxMessages { get { return _configuration.MaxMessages; } }

        public bool UseMaxMessages { get { return _configuration.UseMaxMessages; } }

        public string RemoteMachine { get { return _configuration.RemoteMachine; } }

        public string EventLogName { get { return _configuration.EventLogName; } }
    }
}
