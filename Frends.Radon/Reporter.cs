using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;

namespace Frends.Radon
{
    public class EmailSettings
    {
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("System event report")]
        public string Subject { get; set; }

        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("Frends Radon")]
        public string SenderName { get; set; }

        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("notification_noreply@example.org")]
        public string SenderAddress { get; set; }

        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("john.doe@example.org; jane.doe@example.org")]
        public string Recipients { get; set; }

        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("smtp.somedomain.com")]
        public string SmtpServerName { get; set; }

        [DefaultValue(25)]
        public int PortNumber { get; set; }

        [DefaultValue("false")]
        public bool UseSsl { get; set; }

        [DefaultValue("true")]
        public bool UseWindowsCredentials { get; set; }

        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("")]
        [UIHint(nameof(UseWindowsCredentials), "", false)]
        public string Username { get; set; }

        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("")]
        [UIHint(nameof(UseWindowsCredentials), "", false)]
        [PasswordPropertyText]
        public string Password { get; set; }

        [DefaultValue(1000)]
        public int MaxIdleTime { get; set; }

        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("")]
        public string TemplateFile { get; set; }
    }

    public class FilterSettings
    {
        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("EntryType = \"Error\"")]
        public string FilterString { get; set; }

        [DisplayFormat(DataFormatString = "Expression")]
        [DefaultValue("TimeSpan.FromDays(1)")]
        public TimeSpan MaxTimespan { get; set; }

        [DefaultValue(EventSource.EventLog)]
        public EventSource EventSource { get; set; }

        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("")]
        public string ExternalEventsXml { get; set; }

        [DefaultValue(200)]
        public int MaxMessages { get; set; }

        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("")]
        public string RemoteMachine { get; set; }

        [DisplayFormat(DataFormatString = "Text")]
        [DefaultValue("")]
        public string EventLogName { get; set; }
    }

    public class Reporter
    {
        public static RadonResult Execute([PropertyTab]EmailSettings email, [PropertyTab]FilterSettings filter, CancellationToken cancellationToken)
        {
            var radonExecutor = new RadonExecutor(new RadonConfigurationManager(email, filter));
            var radonProgram = new RadonProgram(radonExecutor);

            var result = radonProgram.ExecuteRadon();

            return new RadonResult
            {
                UserResultMessage = result.UserResultMessage,
                Success = result.Success,
                ActionSkipped = result.ActionSkipped
            };
        }
    }
}
