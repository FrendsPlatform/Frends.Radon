namespace Frends.Radon
{
    public class RadonConfigurationManager : IRadonConfigurationManager
    {
        private readonly EmailConfiguration _emailConfig;

        private readonly FilterConfiguration _filterConfig;

        public RadonConfigurationManager(EmailSettings email, FilterSettings filter)
        {
            _emailConfig = new EmailConfiguration(smtpServerName: email.SmtpServerName,
                                                       senderAddress: email.SenderAddress,
                                                       senderName: email.SenderName,
                                                       portNumber: email.PortNumber,
                                                       maxIdleTime: email.MaxIdleTime,
                                                       useWindowsCredentials: email.UseWindowsCredentials,
                                                       recipients: email.Recipients,
                                                       subject: email.Subject,
                                                       username: email.Username,
                                                       password: email.Password,
                                                       templateFile: email.TemplateFile,
                                                       useSsl: email.UseSsl);

            _filterConfig = new FilterConfiguration(filterString: filter.FilterString,
                                                         timeLimit: filter.MaxTimespan,
                                                         maxMessages: filter.MaxMessages,
                                                         remoteMachine: filter.RemoteMachine,
                                                         eventLogName: filter.EventLogName,
                                                         eventSource: filter.EventSource,
                                                         externalEventsXml: filter.ExternalEventsXml);
        }

        public IEmailConfiguration GetEmailConfig()
        {
            return _emailConfig;
        }

        public IFilterConfiguration GetFilterConfig()
        {
            return _filterConfig;
        }
    }
}
