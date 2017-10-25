using System.ComponentModel;
using Frends.Tasks.Attributes;

namespace Frends.Radon
{
    public class MailerSettings
    {
        [DefaultValue("\"Frends Radon\"")]
        public string SenderName { get; set; }

        [DefaultValue("\"notification_noreply@example.com\"")]
        public string SenderAddress { get; set; }

        [DefaultValue("\"john.doe@example.com; jane.doe@example.com\"")]
        public string Recipients { get; set; }

        [DefaultValue("\"System event report\"")]
        public string Subject { get; set; }

        [DefaultValue("\"You've got mail!\"")]
        public string MessageContent { get; set; }

        [DefaultValue("\"smtp.somedomain.com\"")]
        public string SmtpServerName { get; set; }

        [DefaultValue("25")]
        public int PortNumber { get; set; }

        [DefaultValue("false")]
        public bool UseSsl { get; set; }

        [DefaultValue("true")]
        public bool UseWindowsCredentials { get; set; }

        [ConditionalDisplay(nameof(UseWindowsCredentials), "false")]
        [DefaultValue("\"\"")]
        public string Username { get; set; }

        [ConditionalDisplay(nameof(UseWindowsCredentials), "false")]
        [DefaultValue("\"\"")]
        public string Password { get; set; }

        [DefaultValue(1000)]
        public int MaxIdleTime { get; set; }

        [DefaultValue("\"\"")]
        public string TemplateFile { get; set; }
    }
    public class Mailer
    {
        public static object SendMail(MailerSettings email)
        {
            var config = new EmailConfiguration(smtpServerName: email.SmtpServerName, senderAddress: email.SenderAddress,
                senderName: email.SenderName, portNumber: email.PortNumber, maxIdleTime: email.MaxIdleTime,
                useWindowsCredentials: email.UseWindowsCredentials, recipients: email.Recipients, subject: email.Subject,
                username: email.Username, password: email.Password, useSsl: email.UseSsl);

            using (var messageSender = config.GetReportSender())
            {
                messageSender.SendReport(email.MessageContent, email.Subject);
            }
            return new {
                UserResultMessage = "Email sent with subject '" + email.Subject + "' to " + string.Join(", ", config.Recipients),
                Success = true
            };
        }
    }
}
