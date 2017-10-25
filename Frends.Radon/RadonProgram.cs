using System.Linq;

namespace Frends.Radon
{
    public class RadonProgram
    {
        private readonly IRadonExecutor _executor;

        public RadonProgram(IRadonExecutor executor)
        {
            _executor = executor;
        }

        public RadonResult ExecuteRadon()
        {
            var result = new RadonResult();
            var events = _executor.GetLogEvents();

            if (events.Any())
            {
                _executor.SendReport(events);
                _executor.SaveAlreadyReportedEventIdentification(events.First());

                result.UserResultMessage = string.Format("Mail sent to {0}", this._executor.EmailConfig.RecipientsAsString);
                result.Success = true;
                result.ActionSkipped = false;
            }
            else
            {
                result.UserResultMessage = "No events. No mail has been sent from FRENDS Radon.";
                result.Success = true;
                result.ActionSkipped = true;
            }
            result.UnreportedEventsCount = events.Count;

            return result;
        }
    }
}
