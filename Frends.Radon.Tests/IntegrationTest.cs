using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using nDumbster.smtp;
using NUnit.Framework;
using Rhino.Mocks;

namespace Frends.Radon.Tests
{
    /// <summary>
    /// Integration tests: read the actual event log, and send an email - to the NDumbster SMTP server, though
    /// NOTE: In order to run this test, the user must have rights to the Security log and it must have Logon and Logoff events for the last 12 hours
    /// </summary>
    [TestFixture]
    public class IntegrationTest
    {
        private IEventReader _eventReader;

        private SimpleSmtpServer _smtpServer;

        private EmailConfiguration _currentEmailConfig;

        private IEventIdentificationStore _mockEventIdentificationStore;

        private IRadonExecutor _radonExecutor;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _mockEventIdentificationStore = MockRepository.GenerateMock<IEventIdentificationStore>();
        }

        [SetUp]
        public void Setup()
        {
            _smtpServer = SimpleSmtpServer.Start();

            _currentEmailConfig = new EmailConfiguration(
                                         smtpServerName : "localhost",
                                         portNumber : 25,
                                         subject : "errormail",
                                         recipients : "",
                                         senderAddress : "sender@foo.com");

            _currentEmailConfig.Recipients.Add("user@foo.com");
            _currentEmailConfig.Recipients.Add("user2@foo.com");

            _eventReader = new EventReader(new EventLog("Security"), TimeSpan.FromHours(12));
        }

        [TearDown]
        public void TearDown()
        {
            _smtpServer.Stop();
        }

        [Test]
        public void ShouldReportLogonAndLogoffEventsFilteredByEventId()
        {
            this.SendReportUsingFilter("EventID = 4672 Or EventID = 4700"); // apparently, if you use less than or greater than operators, the email body is encoded in base64 - so don't do that

            Assert.That(_smtpServer.ReceivedEmailCount, Is.EqualTo(1), "An email should have been sent");

            var body = SmtpSenderTest.ParseMessageBody(_smtpServer.ReceivedEmail.Single().Body);

            Assert.That(body, Is.StringContaining("logon"));
        }

        [Test]
        public void ShouldSendEmailsAnonymouslyIfNoUsernameOrPasswordGiven()
        {
            this.SendReportUsingFilter("Message.Contains(\"logged on\")");

            Assert.That(_smtpServer.ReceivedEmailCount, Is.EqualTo(1), "An email should have been sent");

            var body = SmtpSenderTest.ParseMessageBody(_smtpServer.ReceivedEmail.Single().Body);

            Assert.That(body, Is.StringContaining("logon"));
        }

        [Test]
        public void ShouldUseExternalEventsSourceIfSelected()
        {
            var filterConfig = new FilterConfiguration("1=1", TimeSpan.FromHours(12), 100, "", Path.GetRandomFileName(), EventSource.External, "<Events><Event><Message>External event message</Message></Event></Events>");
            var mockConfig = MockRepository.GenerateMock<IRadonConfigurationManager>();
            mockConfig.Expect(c => c.GetEmailConfig()).Return(_currentEmailConfig);
            mockConfig.Expect(c => c.GetFilterConfig()).Return(filterConfig);

            _radonExecutor = new RadonExecutor(mockConfig);
            _radonExecutor.SendReport(_radonExecutor.GetLogEvents());

            Assert.That(_smtpServer.ReceivedEmailCount, Is.EqualTo(1), "An email should have been sent");

            var body = SmtpSenderTest.ParseMessageBody(_smtpServer.ReceivedEmail.Single().Body);

            Assert.That(body, Is.StringContaining("External event message"));
        }

        private void SendReportUsingFilter(string filterString, EventSource source = EventSource.EventLog, string externalEvents = "")
        {
            var filterConfig = new FilterConfiguration(filterString, TimeSpan.FromHours(12), 100, "", "Security", eventSource: source, externalEventsXml: externalEvents);
            var logEventFilter = new LogEventFilter(_eventReader, filterConfig);

            _radonExecutor = new RadonExecutor(_currentEmailConfig, filterConfig, _mockEventIdentificationStore, logEventFilter);
            _radonExecutor.SendReport(_radonExecutor.GetLogEvents());
        }
    }
}
