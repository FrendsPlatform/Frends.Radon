using System;
using System.Linq;
using System.Net;
using NUnit.Framework;

namespace Frends.Radon.Tests
{
    [TestFixture]
    public class EmailConfigurationTest
    {
        private string _smtpServerName;
        private string _senderAddress;
        private string _senderName;
        private int _portNumber;
        private int _maxIdleTime;
        private bool _useWindowsCredentials;
        private string _recipients;
        private string _subject;
        private string _username;
        private string _password;
        private string _templateFile;

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            _smtpServerName = "smtpServerName";
            _senderAddress = "senderAddress@address.com";
            _senderName = "senderName";
            _portNumber = 100;
            _maxIdleTime = 50;
            _useWindowsCredentials = false;
            _recipients = "recipients@address.com";
            _subject = "subject";
            _username = "username";
            _password = "password";
            _templateFile = "templateFile";
        }

        [Test]
        public void EmailConfiguration_ShouldReturnCorrectValues()
        {
            var emailConfiguration = new EmailConfiguration(smtpServerName: _smtpServerName,
                senderAddress: _senderAddress,
                senderName: _senderName,
                portNumber: _portNumber,
                maxIdleTime: _maxIdleTime,
                useWindowsCredentials: _useWindowsCredentials,
                recipients: _recipients,
                subject: _subject,
                username: _username,
                password: _password,
                templateFile: _templateFile);

            Assert.That(emailConfiguration.SmtpServerName, Is.EqualTo(_smtpServerName));
            Assert.That(emailConfiguration.SenderAddress, Is.EqualTo(_senderAddress));
            Assert.That(emailConfiguration.SenderName, Is.EqualTo(_senderName));
            Assert.That(emailConfiguration.Subject, Is.EqualTo(_subject));
            Assert.That(emailConfiguration.TemplateFile, Is.EqualTo(_templateFile));
            Assert.That(emailConfiguration.UseWindowsCredentials, Is.EqualTo(_useWindowsCredentials));
            Assert.That(emailConfiguration.PortNumber, Is.EqualTo(_portNumber));
            Assert.That(emailConfiguration.MaxIdleTime, Is.EqualTo(_maxIdleTime));
            Assert.That(emailConfiguration.Username, Is.EqualTo(_username));
            Assert.That(emailConfiguration.Password, Is.EqualTo(_password));
            Assert.That(emailConfiguration.Recipients.Count, Is.EqualTo(1));
            Assert.That(emailConfiguration.Recipients.Single(), Is.EqualTo(_recipients));
        }

        [Test]
        public void EmailConfiguration_ShouldTrimSmtpSenderSubjectAndTemplateFileValues()
        {
            const string smtp = " mySmtp ";
            const string senderAddress = " senderAddress  ";
            const string senderName = "    senderName  ";
            const string subject = "    subject   ";
            const string templateFile = "    templateFile  ";

            var emailConfiguration = new EmailConfiguration(smtpServerName: smtp, 
                senderAddress: senderAddress, 
                senderName: senderName, 
                subject: subject, 
                templateFile: templateFile);

            Assert.That(emailConfiguration.SmtpServerName, Is.EqualTo(smtp.Trim()));
            Assert.That(emailConfiguration.SenderAddress, Is.EqualTo(senderAddress.Trim()));
            Assert.That(emailConfiguration.SenderName, Is.EqualTo(senderName.Trim()));
            Assert.That(emailConfiguration.Subject, Is.EqualTo(subject.Trim()));
            Assert.That(emailConfiguration.TemplateFile, Is.EqualTo(templateFile.Trim()));
        }

        [Test]
        public void EmailConfiguration_ShouldNotTrimUsernameAndPasswordValues()
        {
            const string username = " myUserName ";
            const string password = " myPassword  ";

            var emailConfiguration = new EmailConfiguration(username: username, 
                password: password);

            Assert.That(emailConfiguration.Username, Is.EqualTo(username));
            Assert.That(emailConfiguration.Password, Is.EqualTo(password));
        }

        [Test]
        public void EmailConfiguration_ShouldParseOneRecipientCorrectly()
        {
            const string recipient = "veijo@frends.com";

            var config = new EmailConfiguration(recipients: recipient);

            Assert.That(config.Recipients.Count, Is.EqualTo(1));
            Assert.That(config.Recipients.Contains(recipient));
        }

        [Test]
        public void EmailConfiguration_ShouldTrimOneRecipientCorrectly()
        {
            const string recipient = "  veijo@frends.com    ";

            var config = new EmailConfiguration(recipients: recipient);

            Assert.That(config.Recipients.Count, Is.EqualTo(1));
            Assert.That(config.Recipients.Contains(recipient.Trim()));
        }

        [Test]
        public void EmailConfiguration_ShouldParseOneRecipientCorrectlyAndIgnoreWhitespace()
        {
            const string recipient = "veijo@frends.com;    ";

            var config = new EmailConfiguration(recipients: recipient);

            Assert.That(config.Recipients.Count, Is.EqualTo(1));
            Assert.That(recipient.Contains(config.Recipients[0]));
        }

        [Test]
        public void EmailConfiguration_ShouldParseTwoRecipientsCorrectly()
        {
            const string twoRecipients = "veijo@frends.com;alma@frends.com";

            var config = new EmailConfiguration(recipients: twoRecipients);

            Assert.That(config.Recipients.Count, Is.EqualTo(2));
            Assert.That(twoRecipients.Contains(config.Recipients[0]));
            Assert.That(twoRecipients.Contains(config.Recipients[1]));
        }

        [Test]
        public void CreateEmailConfiguration_ShouldIgnoreWhitespaceAndParseTwoRecipientsCorrectly()
        {
            const string twoRecipientsAndWhitespace = "veijo@frends.com;    ;alma@frends.com";

            var config = new EmailConfiguration(recipients: twoRecipientsAndWhitespace);

            Assert.That(config.Recipients.Count, Is.EqualTo(2));
            Assert.That(twoRecipientsAndWhitespace.Contains(config.Recipients[0]));
            Assert.That(twoRecipientsAndWhitespace.Contains(config.Recipients[1]));
        }

        [Test]
        public void EmailConfiguration_ShouldReturnStringEmptyForSmtpSenderSubjectAndTemplateFileIfNullPassed()
        {
            var emailConfiguration = new EmailConfiguration(smtpServerName: null,
                senderAddress: null,
                senderName: null,
                subject: null,
                templateFile: null);

            Assert.That(emailConfiguration.SmtpServerName, Is.EqualTo(string.Empty));
            Assert.That(emailConfiguration.SenderAddress, Is.EqualTo(string.Empty));
            Assert.That(emailConfiguration.SenderName, Is.EqualTo(string.Empty));
            Assert.That(emailConfiguration.Subject, Is.EqualTo(string.Empty));
            Assert.That(emailConfiguration.TemplateFile, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetReportSender_ShouldGetReportSender()
        {
            var emailConfiguration = new EmailConfiguration(smtpServerName: _smtpServerName,
                senderAddress: _senderAddress,
                senderName: _senderName,
                portNumber: _portNumber,
                maxIdleTime: _maxIdleTime,
                useWindowsCredentials: _useWindowsCredentials,
                recipients: _recipients,
                subject: _subject,
                username: _username,
                password: _password,
                templateFile: _templateFile);

            var smtpSender = emailConfiguration.GetReportSender();
            Assert.That(smtpSender.GetType(), Is.EqualTo(typeof(SmtpSender)));

            var smtp = (SmtpSender) smtpSender;
            Assert.That(smtp.SmtpClient.Host, Is.EqualTo(_smtpServerName));
            Assert.That(smtp.SmtpClient.Port, Is.EqualTo(_portNumber));
            Assert.That(smtp.SmtpClient.UseDefaultCredentials, Is.EqualTo(_useWindowsCredentials));
            Assert.That(smtp.SmtpClient.ServicePoint.MaxIdleTime, Is.EqualTo(_maxIdleTime));
            Assert.That(smtp.Recipients.Count, Is.EqualTo(1));
            Assert.That(smtp.Recipients[0].Address, Is.EqualTo(_recipients));
            Assert.That(smtp.Sender.Address, Is.EqualTo(_senderAddress));
        }

        [Test]
        public void GetReportSender_ShouldThrowArgumentExceptionIfSmtpServerNameMissing()
        {
            var emailConfiguration = new EmailConfiguration(smtpServerName: null,
                senderAddress: _senderAddress,
                senderName: _senderName,
                portNumber: _portNumber,
                maxIdleTime: _maxIdleTime,
                useWindowsCredentials: _useWindowsCredentials,
                recipients: _recipients,
                subject: _subject,
                username: _username,
                password: _password,
                templateFile: _templateFile);

            var exception = Assert.Throws<ArgumentException>(() => emailConfiguration.GetReportSender());
            Assert.That(exception.Message, Is.StringContaining("Smtp server"));
        }

        [Test]
        public void GetReportSender_ShouldThrowArgumentExceptionIfThereAreNoRecipients()
        {
            var emailConfiguration = new EmailConfiguration(smtpServerName: _smtpServerName,
                senderAddress: _senderAddress,
                senderName: _senderName,
                portNumber: _portNumber,
                maxIdleTime: _maxIdleTime,
                useWindowsCredentials: _useWindowsCredentials,
                recipients: null,
                subject: _subject,
                username: _username,
                password: _password,
                templateFile: _templateFile);

            var exception = Assert.Throws<ArgumentException>(() => emailConfiguration.GetReportSender());
            Assert.That(exception.Message, Is.StringContaining("A recipient"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void GetReportSender_ShouldThrowArgumentExceptionIfSenderAddressMissing(string badSenderAddress)
        {
            var emailConfiguration = new EmailConfiguration(smtpServerName: _smtpServerName,
                senderAddress: badSenderAddress, 
                senderName: _senderName, 
                portNumber: _portNumber,
                maxIdleTime: _maxIdleTime, 
                useWindowsCredentials: _useWindowsCredentials, 
                recipients: _recipients, 
                subject: _subject, 
                username: _username, 
                password: _password, 
                templateFile: _templateFile);

            var exception = Assert.Throws<ArgumentException>(() => emailConfiguration.GetReportSender());
            Assert.That(exception.Message, Is.StringContaining("Sender address"));
        }

        [Test]
        public void GetReportSender_UserNameAndPasswordShouldCreateNewCredentials()
        {
            var emailConfig = new EmailConfiguration(smtpServerName: "smtp.somedomain.com",
                                                     recipients: "veijo@frends.com",
                                                     username: "johndoe",
                                                     password: "secretWord",
                                                     useWindowsCredentials: false,
                                                     senderAddress: "veijo@frends.com");

            var reportSender = (SmtpSender)emailConfig.GetReportSender();
            var credential = (NetworkCredential)reportSender.SmtpClient.Credentials;

            Assert.AreEqual(emailConfig.Username, credential.UserName, "Username should be the same as in config");
            Assert.AreEqual(emailConfig.Password, credential.Password, "Password should be the same as in config");
        }

        [Test]
        public void GetReportSender_EmptyPasswordShouldThrowErrorWhenWindowsCredentialsNotUsed()
        {
            var emailConfig = new EmailConfiguration(smtpServerName: "smtp.somedomain.com",
                                                     username: "johndoe",
                                                     useWindowsCredentials: false);

            var e = Assert.Throws<ArgumentException>(() => emailConfig.GetReportSender());

            Assert.That(e.Message, Is.StringContaining("password").IgnoreCase);
        }

        [Test]
        public void GetReportSender_WindowsCredentialsSetToTrueShouldIgnoreGivenUsernameAndPassword()
        {
            var emailConfig = new EmailConfiguration(smtpServerName: "smtp.somedomain.com",
                                                     recipients: "veijo@frends.com",
                                                     username: "johndoe",
                                                     password: "secretWord",
                                                     senderAddress: "veijo@frends.com");

            var reportSender = (SmtpSender)emailConfig.GetReportSender();
            var credential = (NetworkCredential)reportSender.SmtpClient.Credentials;

            Assert.IsEmpty(credential.UserName, "Username should be empty");
            Assert.IsEmpty(credential.Password, "Password should be empty");
        }

        [Test]
        public void GetReportSender_WindowsCredentialsSetToFalseWithUsernameGivenShouldSetCredentials()
        {
            var emailConfig = new EmailConfiguration(smtpServerName: "smtp.somedomain.com",
                                                     recipients: "veijo@frends.com",
                                                     username: "johndoe",
                                                     password: "password",
                                                     useWindowsCredentials: false,
                                                     senderAddress: "veijo@frends.com");

            var reportSender = (SmtpSender)emailConfig.GetReportSender();
            var credential = (NetworkCredential)reportSender.SmtpClient.Credentials;

            Assert.IsNotEmpty(credential.UserName, "Username should NOT be empty");
            Assert.IsNotEmpty(credential.Password, "Password should NOT be empty");
        }

        [Test]
        public void GetReportSender_EmptyUserAndFalseCredentialsShouldUseAnonymousAccess()
        {
            var emailConfig = new EmailConfiguration(smtpServerName: "smtp.somedomain.com",
                                                     recipients: "veijo@frends.com",
                                                     useWindowsCredentials: false,
                                                     senderAddress: "veijo@frends.com");

            var reportSender = (SmtpSender)emailConfig.GetReportSender();

            Assert.IsNull(reportSender.SmtpClient.Credentials, "Credentials should be empty");
        }
    }
}
