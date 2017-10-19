using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace Frends.Radon
{
    public interface IEmailConfiguration
    {
        string SmtpServerName { get; }

        string SenderAddress { get; }

        string SenderName { get; }

        string Subject { get; }

        string TemplateFile { get; }

        IList<string> Recipients { get; }

        string RecipientsAsString { get; }

        bool UseWindowsCredentials { get; }

        int PortNumber { get; }

        int MaxIdleTime { get; }

        string Username { get; }

        string Password { get; }

        IReportSender GetReportSender();
    }

    public class EmailConfiguration : IEmailConfiguration
    {
        public string SmtpServerName { get; private set; }

        public string SenderAddress { get; private set; }

        public string SenderName { get; private set; }

        public string Subject { get; private set; }

        public string TemplateFile { get; private set; }

        public IList<string> Recipients { get; private set; }

        public string RecipientsAsString { get; private set; }

        public bool UseWindowsCredentials { get; private set; }

        public int PortNumber { get; private set; }

        public int MaxIdleTime { get; private set; }

        public string Username { get; private set; }

        public string Password { get; private set; }

        public bool UseSsl { get; private set; }

        public EmailConfiguration(string smtpServerName = null,
                                  string senderAddress = null,
                                  string senderName = null,
                                  int portNumber = 25,
                                  int maxIdleTime = 10000,
                                  bool useWindowsCredentials = true,
                                  string recipients = null,
                                  string subject = null,
                                  string username = null,
                                  string password = null,
                                  string templateFile = null,
                                  bool useSsl = false)
        {
            SmtpServerName = Parser.TrimLineOrReturnStringEmptyIfNull(smtpServerName);
            SenderAddress = Parser.TrimLineOrReturnStringEmptyIfNull(senderAddress);
            SenderName = Parser.TrimLineOrReturnStringEmptyIfNull(senderName);
            PortNumber = portNumber;
            MaxIdleTime = maxIdleTime;
            UseWindowsCredentials = useWindowsCredentials;
            UseSsl = useSsl;
            Recipients = Parser.ParseRecipientsToList(recipients);
            RecipientsAsString = Parser.ParseRecipientsToString(Recipients);
            Subject = Parser.TrimLineOrReturnStringEmptyIfNull(subject);
            TemplateFile = Parser.TrimLineOrReturnStringEmptyIfNull(templateFile);
            Username = username;
            Password = password;
        }

        public IReportSender GetReportSender()
        {
            if (!string.IsNullOrEmpty(Username) && !UseWindowsCredentials && string.IsNullOrEmpty(Password))
            {
                throw new ArgumentException("Password cannot be empty when using username/password authentication");
            }
            if (string.IsNullOrEmpty(SenderAddress))
            {
                throw new ArgumentException("Sender address cannot be empty.");
            }
            if (!Recipients.Any())
            {
                throw new ArgumentException("A recipient must be specified.");
            }
            if (string.IsNullOrEmpty(SmtpServerName))
            {
                throw new ArgumentException("Smtp server has to be defined.");
            }

            SmtpSender smtp;

            if (string.IsNullOrEmpty(Username) || UseWindowsCredentials)
            {
                // In these two cases the username and password should not be given to the SmtpSender:
                // - if user name is empty -> use anonymous access (only if the UseWindowsCredentials is false)
                // - if UseWindowsCredentials is true -> use the windows credentials                
                smtp = new SmtpSender(SmtpServerName, PortNumber, UseWindowsCredentials, MaxIdleTime, UseSsl);
            }
            else
            {
                smtp = new SmtpSender(SmtpServerName, PortNumber, UseWindowsCredentials, MaxIdleTime, Username, Password, UseSsl);
            }

            smtp.Sender = new MailAddress(SenderAddress, SenderName);
            smtp.Recipients = Recipients.Select(m => new MailAddress(m)).ToList();

            return smtp;
        }
    }
}