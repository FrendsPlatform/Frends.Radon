using System;
using System.ComponentModel;
using System.Threading;
using Frends.Tasks.Attributes;

namespace Frends.Radon
{
    public class EmailSettings
    {
        [DefaultValue("\"System event report\"")]
        public string Subject { get; set; }

        [DefaultValue("\"Frends Radon\"")]
        public string SenderName { get; set; }

        [DefaultValue("\"notification_noreply@example.org\"")]
        public string SenderAddress { get; set; }

        [DefaultValue("\"john.doe@example.org; jane.doe@example.org\"")]
        public string Recipients { get; set; }

        [DefaultValue("\"smtp.somedomain.com\"")]
        public string SmtpServerName { get; set; }

        [DefaultValue("25")]
        public int PortNumber { get; set; }

        [DefaultValue("false")]
        public bool UseSsl { get; set; }

        [DefaultValue("true")]
        public bool UseWindowsCredentials { get; set; }

        [DefaultValue("\"\"")]
        [ConditionalDisplay(nameof(UseWindowsCredentials), "false")]
        public string Username { get; set; }

        [DefaultValue("\"\"")]
        [ConditionalDisplay(nameof(UseWindowsCredentials), "false")]
        [PasswordPropertyText]
        public string Password { get; set; }

        [DefaultValue(1000)]
        public int MaxIdleTime { get; set; }

        [DefaultValue("\"\"")]
        public string TemplateFile { get; set; }
    }

    public class FilterSettings
    {
        [DefaultValue("\"EntryType = \\\"Error\\\"\"")]
        public string FilterString { get; set; }

        [DefaultValue("TimeSpan.FromDays(1)")]
        public TimeSpan MaxTimespan { get; set; }

        [DefaultValue(EventSource.EventLog)]
        public EventSource EventSource { get; set; }

        [DefaultValue("\"\"")]
        public string ExternalEventsXml { get; set; }

        [DefaultValue(200)]
        public int MaxMessages { get; set; }

        [DefaultValue("\"\"")]
        public string RemoteMachine { get; set; }

        [DefaultValue("\"\"")]
        public string EventLogName { get; set; }
    }

    public class Reporter
    {
        public static RadonResult Execute([CustomDisplay(DisplayOption.Tab)]EmailSettings email, [CustomDisplay(DisplayOption.Tab)]FilterSettings filter, CancellationToken cancellationToken)
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
