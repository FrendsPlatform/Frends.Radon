using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;

namespace Frends.Radon
{
    public class SmtpSender : IReportSender
    {
        public readonly SmtpClient SmtpClient;

        public MailAddress Sender { get; set; }

        public List<MailAddress> Recipients { get; set; }

        private bool _disposed;

        public SmtpSender(string serverName, int portNumber, bool useWindowsCredentials, int maxIdleTime, bool useSsl)
        {
            //if default credentials is set to 'true', the currently logged user's credentials are used
            //if this is set to 'false' mail is sen anonymously
            SmtpClient = new SmtpClient(serverName, portNumber != 0 ? portNumber : 25)
            {
                UseDefaultCredentials = useWindowsCredentials,
                EnableSsl = useSsl
            };
            SmtpClient.ServicePoint.MaxIdleTime = maxIdleTime;
            Recipients = new List<MailAddress>();
        }

        public SmtpSender(string serverName, int portNumber, bool useWindowsCredentials, int maxIdleTime, string username, string password, bool useSsl) : this(serverName, portNumber, useWindowsCredentials, maxIdleTime, useSsl)
        {
            SmtpClient.Credentials = new System.Net.NetworkCredential(username, password);
        }

        public void SendReport(string logDataHtml, string subject)
        {
            using (var mailMessage = new MailMessage { From = Sender, Subject = subject, IsBodyHtml = true, Body = logDataHtml })
            {
                foreach (var receiver in Recipients)
                {
                    mailMessage.To.Add(receiver.Address);
                }

                Trace.WriteLine("Sending mail from FRENDS Radon to " + mailMessage.To);
                try
                {
                    SmtpClient.Send(mailMessage);
                    Trace.WriteLine("Mail has been sent from FRENDS Radon.");
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Error while sending mail from FRENDS Radon: " + e);
                    throw new Exception("Error while sending mail from FRENDS Radon: " + e.Message, e);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // free other managed objects that implement
                // IDisposable only

                SmtpClient.Dispose();
            }
            _disposed = true;
        }
    }
}