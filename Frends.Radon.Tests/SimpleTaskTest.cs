using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using nDumbster.smtp;
using NUnit.Framework;

namespace Frends.Radon.Tests
{
    public class SimpleTaskTest
    {
        SimpleSmtpServer smtpServer;

        [SetUp]
        public void Setup()
        {
            smtpServer = SimpleSmtpServer.Start();
            smtpServer.ClearReceivedEmail();
        }

        [TearDown]
        public void TearDown()
        {
            smtpServer.Stop();
        }

        private void WriteToLogWithStandardLog(string eventName, EventLogEntryType entryType, int eventId)
        {
            WriteToLog("Application", "testSource", eventName, entryType, eventId, null);
        }

        private void WriteToLogWithCustomLog(string logName, string sourceName, string eventName, string remoteMachine)
        {
            WriteToLog(logName, sourceName, eventName, EventLogEntryType.Error, 666, remoteMachine);
        }

        private void WriteToLog(string logName, string sourceName, string eventName, EventLogEntryType entryType, int eventId, string remoteMachine)
        {
            var log = String.IsNullOrEmpty(remoteMachine) ? new EventLog(logName) : new EventLog(logName, remoteMachine);

            log.Source = sourceName;

            log.WriteEntry(eventName, entryType, eventId);
        }

        [Test]
        public void CanRunAndReturnsUserResultSuccess()
        {
            const string eventName = "RadonWFTestMessage CanRunAndReturnsUserResultSuccess";
            WriteToLogWithStandardLog(eventName, EventLogEntryType.Error, 666);
            var outParams = RunWorkflow(GetEmailSettings(), GetFilterSettings("EntryType = \"Error\" And EventID = 666"));

            Assert.That(outParams.UserResultMessage, Iz.EqualTo("Mail sent to bar@foo.com"));
            Assert.That(smtpServer.ReceivedEmailCount, Is.EqualTo(1));
            var body = SmtpSenderTest.ParseMessageBody(smtpServer.ReceivedEmail.Single().Body);

            Assert.That(body, Is.StringContaining(eventName));
            smtpServer.ClearReceivedEmail();
        }

        private EmailSettings GetEmailSettings()
        {
            return new EmailSettings
            {
                SmtpServerName = "localhost",
                MaxIdleTime = 5000,
                PortNumber = 25,
                Subject = "Test",
                SenderAddress = "foo@bar.com",
                UseWindowsCredentials = true,
                SenderName = "foo",
                Recipients = "bar@foo.com"
            };
        }

        private FilterSettings GetFilterSettings(string filter, string remoteMachine = "", string eventLogName = "")
        {
            return new FilterSettings
            {
                FilterString = filter,
                EventSource = EventSource.EventLog,
                EventLogName = eventLogName,
                ExternalEventsXml = "",
                MaxMessages = 100,
                MaxTimespan = TimeSpan.FromHours(1),
                RemoteMachine = remoteMachine
            };
        }

        [Test]
        public void CanRunAndReturnsUserResultNoMailSent()
        {
            var outParams = RunWorkflow(GetEmailSettings(), GetFilterSettings("EntryType = \"Error\" And EventID = 667"));

            Assert.That(outParams.UserResultMessage, Iz.EqualTo("No events. No mail has been sent from FRENDS Radon."));
            Assert.That(smtpServer.ReceivedEmailCount, Iz.LessThan(1));
            smtpServer.ClearReceivedEmail();
        }

        [Test]
        public void AcceptsNullForFilter_remoteMachineParameterAndFilter_eventLogName()
        {
            var outParams = RunWorkflow(GetEmailSettings(), GetFilterSettings("", null, null));

            Assert.That(outParams.Success, Iz.True);
            smtpServer.ClearReceivedEmail();
        }

        [Test]
        public void ShouldFailIfInvalidFilterString()
        {
            var filterString = "EmtryType = \"Epror\"";
            var ex = Assert.Throws(typeof(ArgumentException), () => RunWorkflow(GetEmailSettings(), GetFilterSettings(filterString)));

            Assert.That(ex.Message, Is.StringContaining(filterString));
            smtpServer.ClearReceivedEmail();
        }

        [Test]
        public void ShouldFailIfInvalidEventLogName()
        {
            string logName = "Apppppppplication";
            var ex = Assert.Throws(typeof(ArgumentException), () => RunWorkflow(GetEmailSettings(), GetFilterSettings("", "", logName)));

            Assert.That(ex.Message, Is.StringContaining(logName));
            smtpServer.ClearReceivedEmail();
        }

        [Test]
        public void ShouldFailIfInvalidMachineName()
        {
            string machine = "FooBarShouldNotExist";
            var ex = Assert.Throws(typeof(ArgumentException), () => RunWorkflow(GetEmailSettings(), GetFilterSettings("", machine, "")));

            smtpServer.ClearReceivedEmail();
            Assert.That(ex.Message, Is.StringContaining(machine));
            smtpServer.ClearReceivedEmail();
        }

        [Test]
        public void CanReadOtherThanApplicationLog()
        {
            const string eventName = "TestEvent CanReadOtherThanApplicationLog";
            WriteToLogWithCustomLog("TestLog", "TestLogSource", eventName, null);

            var outParams = RunWorkflow(GetEmailSettings(), GetFilterSettings("", "", "TestLog"));

            Assert.That(outParams.UserResultMessage, Iz.EqualTo("Mail sent to bar@foo.com"));
            Assert.That(smtpServer.ReceivedEmailCount, Is.EqualTo(1));

            var body = SmtpSenderTest.ParseMessageBody(smtpServer.ReceivedEmail.Single().Body);
            Assert.That(body, Is.StringContaining(eventName));
            smtpServer.ClearReceivedEmail();
        }


        //While this test does not actually read from a remote server, 
        // it should make our code act as it was connecting to a remote machine
        [Test]
        public void CanReadRemoteEventLog()
        {
            const string eventName = "TestEvent CanReadRemoteEventLog";
            WriteToLogWithCustomLog("Application", "testSource", eventName, "FRBLD01");

            var outParams = RunWorkflow(GetEmailSettings(), GetFilterSettings("EntryType = \"Error\" And EventID = 666", "FRBLD01", ""));

            Assert.That(outParams.UserResultMessage, Iz.EqualTo("Mail sent to bar@foo.com"));
            Assert.That(smtpServer.ReceivedEmailCount, Is.EqualTo(1));

            var body = SmtpSenderTest.ParseMessageBody(smtpServer.ReceivedEmail.Single().Body);
            Assert.That(body, Is.StringContaining(eventName));
            smtpServer.ClearReceivedEmail();
        }

        [Test]
        public void ShouldReturnResultsWithMaxResultsZero()
        {
            const string eventName = "RadonWFTestMessage ShouldReturnResultsWithMaxResultsZero";
            WriteToLogWithStandardLog(eventName, EventLogEntryType.Error, 666);

            var filterSettings = GetFilterSettings("EntryType = \"Error\" And EventID = 666", "", "");
            filterSettings.MaxMessages = 0;

            var outParams = RunWorkflow(GetEmailSettings(), filterSettings);

            Assert.That(outParams.UserResultMessage, Iz.EqualTo("Mail sent to bar@foo.com"));
            Assert.That(smtpServer.ReceivedEmailCount, Is.EqualTo(1));

            var body = SmtpSenderTest.ParseMessageBody(smtpServer.ReceivedEmail.Single().Body);
            Assert.That(body, Is.StringContaining(eventName));
            smtpServer.ClearReceivedEmail();
        }

        private bool RunWorkflowAndExpectException(Type expectedExceptionType, EmailSettings emailSettings, FilterSettings filterSettings)
        {
            try
            {

                Reporter.Execute(emailSettings, filterSettings, new CancellationToken());
            }
            catch (Exception ex)
            {
                if (expectedExceptionType == ex.GetType())
                {
                    return true;
                }
            }
            return false;
        }

        private RadonResult RunWorkflow(EmailSettings emailSettings, FilterSettings filterSettings)
        {
            var result = Reporter.Execute(emailSettings, filterSettings, new CancellationToken());

            return result;
        }
        [Test]
        public void ThrowsArgumentExceptionIfNoSmtpServerDefined()
        {
            var eventName = "RadonWFTestMessage ThrowsArgumentExceptionIfNoSmtpServerDefined";
            WriteToLogWithStandardLog(eventName, EventLogEntryType.Error, 666);

            var filter = GetFilterSettings("EntryType = \"Error\" And EventID = 666");
            var email = GetEmailSettings();
            email.SmtpServerName = "";

            var result = RunWorkflowAndExpectException(typeof(ArgumentException), email, filter);

            Assert.That(result, "Argument exception wasn't thrown.");


            smtpServer.ClearReceivedEmail();
        }



        [Test]
        public void CanRunAndReturnsUserResultSuccessWithNullFilterString()
        {
            const string eventName = "RadonWFTestMessage CanRunAndReturnsUserResultSuccessWithNullFilterString";
            WriteToLogWithStandardLog(eventName, EventLogEntryType.Error, 666);

            var outParams = RunWorkflow(GetEmailSettings(), GetFilterSettings(null, "", ""));

            Assert.That(outParams.UserResultMessage, Is.EqualTo("Mail sent to bar@foo.com"));
            Assert.That(smtpServer.ReceivedEmailCount, Is.EqualTo(1));

            var body = SmtpSenderTest.ParseMessageBody(smtpServer.ReceivedEmail.Single().Body);
            Assert.That(body, Is.StringContaining(eventName));
            smtpServer.ClearReceivedEmail();
        }

        [Test]
        public void CanRunAndReturnsUserResultSuccessWithEmptyFilterString()
        {
            const string eventName = "RadonWFTestMessage CanRunAndReturnsUserResultSuccessWithEmptyFilterString";
            WriteToLogWithStandardLog(eventName, EventLogEntryType.Error, 666);

            var outParams = RunWorkflow(GetEmailSettings(), GetFilterSettings("", "", ""));

            Assert.That(outParams.UserResultMessage, Iz.EqualTo("Mail sent to bar@foo.com"));
            Assert.That(smtpServer.ReceivedEmailCount, Is.EqualTo(1));

            var body = SmtpSenderTest.ParseMessageBody(smtpServer.ReceivedEmail.Single().Body);
            Assert.That(body, Is.StringContaining(eventName));
            smtpServer.ClearReceivedEmail();
        }

    }
}
