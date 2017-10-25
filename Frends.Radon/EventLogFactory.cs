using System;
using System.Diagnostics;

namespace Frends.Radon
{
    public interface IEventLogFactory
    {
        EventLog CreateEventLog(IFilterConfiguration filterConfig);
    }

    public class EventLogFactory : IEventLogFactory
    {
        public EventLog CreateEventLog(IFilterConfiguration filterConfig)
        {
            var eventLogName = String.IsNullOrEmpty(filterConfig.EventLogName) ? "Application" : filterConfig.EventLogName;
            var remoteMachineSet = !String.IsNullOrEmpty(filterConfig.RemoteMachine);
            EventLog log;
            try
            {
                if (!remoteMachineSet)
                    log =  new EventLog(eventLogName);
                else
                    log = new EventLog(eventLogName, filterConfig.RemoteMachine);

                //try to use the log to check that it is valid:

                var test = log.Entries.Count;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(String.Format("Could not create event log reader for log '{0}' on the {1} machine {2}. Got exception: {3}", 
                    eventLogName, 
                    remoteMachineSet ? "remote" : "local", 
                    remoteMachineSet ? filterConfig.RemoteMachine : Environment.MachineName,
                    ex.Message), ex);
            }
            return log;
        }
    }
}
