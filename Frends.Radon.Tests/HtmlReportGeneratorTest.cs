using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Frends.Radon.TemplateDrops;
using NUnit.Framework;

namespace Frends.Radon.Tests
{
    [TestFixture]
    public class HtmlReportGeneratorTest
    {
        private IList<LogEvent> _events;
        private FilterConfiguration _filterConfig;
        private EmailConfiguration _emailConfig;

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
             _events = new List<LogEvent>
                          {
                              new LogEvent
                                  {
                                      Category = "inside",
                                      CategoryNumber = 1,
                                      EntryType = EventLogEntryType.Warning,
                                      Message = "inside",
                                      Source = "inside",
                                      TimeGenerated = DateTime.Now.AddMinutes(-3)
                                  },
                              new LogEvent
                                  {
                                      Category = "inside2",
                                      CategoryNumber = 1,
                                      EntryType = EventLogEntryType.Warning,
                                      Message = "inside2",
                                      Source = "inside2",
                                      TimeGenerated = DateTime.Now.AddMinutes(-3)
                                  }
                          };
        }

        [SetUp]
        public void Setup()
        {
            SetupFilterConfig();
            SetupEmailConfig();
        }

        private void SetupEmailConfig()
        {
            _emailConfig = new EmailConfiguration {};
        }

        private void SetupFilterConfig(string eventLogName = null, string remoteMachineName = null)
        {
             _filterConfig = new FilterConfiguration("testFilterString", TimeSpan.FromDays(1), 100, remoteMachineName, eventLogName);
        }

        [Test]
        public void CreateHtmlReport_ShouldReturnItemsInTable()
        {

            HtmlReportGenerator target = new HtmlReportGenerator(_events, _filterConfig, _emailConfig);

            string result = target.CreateHtmlReport();

            Assert.That(result, Is.StringContaining("inside"));
            Assert.That(result, Is.StringContaining("inside2"));
            Assert.That(result, Is.StringContaining("<table"));
            Assert.That(result, Is.StringContaining("</table>"));
        }

        [Test]
        public void CreateHtmlReport_ShouldGiveDefaultLogNameInHeader()
        {
            HtmlReportGenerator target = new HtmlReportGenerator(_events, _filterConfig, _emailConfig); //Uses default log

            string result = target.CreateHtmlReport();

            Assert.That(result, Is.StringContaining("Application")); //Application is the default log
        }

        [Test]
        public void CreateHtmlReport_ShouldGiveSpecificLogNameInHeader()
        {
            HtmlReportGenerator target = new HtmlReportGenerator(_events, new FilterConfiguration("", new TimeSpan(), 10, null, "Security"), _emailConfig);

            string result = target.CreateHtmlReport();

            Assert.That(result, Is.StringContaining("Security"));
        }

        [Test]
        public void CreateHtmlReport_ShouldContainTargetMachineNameInHeader()
        {
            HtmlReportGenerator target = new HtmlReportGenerator(_events, new FilterConfiguration("", new TimeSpan(), 10, "testRemoteMachineName", null), _emailConfig);

            string result = target.CreateHtmlReport();

            Assert.That(result, Is.StringContaining("testRemoteMachineName"));
        }

        [Test]
        public void CreateHtmlReport_ShouldContainLocalMachineInHeaderIfEmptyRemoteMachine()
        {
            HtmlReportGenerator target = new HtmlReportGenerator(_events, new FilterConfiguration("", new TimeSpan(), 10, "", null), _emailConfig);

            string result = target.CreateHtmlReport();

            Assert.That(result, Is.StringContaining(Environment.MachineName));
        }
        [Test]
        public void ShouldHaveHeader()
        {
            HtmlReportGenerator target = new HtmlReportGenerator(_events, _filterConfig, _emailConfig);

            string result = target.CreateHtmlReport();

            Assert.That(result, Is.StringContaining("events from the Application log from"));
            Assert.That(result, Is.StringContaining(DateTime.Now.Date.ToShortDateString()));
            Assert.That(result, Is.StringContaining(DateTime.Now.AddDays(-1).Date.ToShortDateString()));
        }

        [Test]
        public void ShouldTellTheFilterString()
        {
            HtmlReportGenerator target = new HtmlReportGenerator(_events, _filterConfig, _emailConfig);

            string result = target.CreateHtmlReport();

            Assert.That(result, Is.StringContaining("testFilterString"));
        }

        [Test]
        public void ShouldUseShortDateStringsForContext()
        {
            DateTime expectedDateTime = DateTime.Now;
            var context = HtmlReportGenerator.CreateContextHash(_filterConfig, new LogEventDrop[]{});

            Assert.That(context["start_time"], Is.StringContaining(expectedDateTime.AddDays(-1).ToString("g")), "The header should contain the end time (the current time) in short date time format");
            Assert.That(context["current_time"], Is.StringContaining(expectedDateTime.ToString("g")), "The header should contain the end time (the current time) in short date time format");
        }

        [TestCase(null, "Application")]
        [TestCase("", "Application")]
        [TestCase("Security", "Security")]
        public void ShouldSetEventLogNameCorrectlyToContext(string configured, string expected)
        {
            SetupFilterConfig(eventLogName:configured);
            var context = HtmlReportGenerator.CreateContextHash(_filterConfig, new LogEventDrop[] { });

            Assert.That(context["event_log_name"], Is.EqualTo(expected));
        }

        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase("Foo", "Foo")]
        public void ShouldSetMachineNameCorrectlyToContext(string configured, string expected)
        {
            SetupFilterConfig(remoteMachineName: configured);

            var context = HtmlReportGenerator.CreateContextHash(_filterConfig, new LogEventDrop[] {});

            Assert.That(context["machine_name"], Is.EqualTo(expected ?? Environment.MachineName));
        }

        [Test]
        public void ShouldIndicateMaxMessagesNotReached()
        {
            var target =
                new HtmlReportGenerator(
                    Enumerable.Range(0, _filterConfig.MaxMessages-1).Select(i => new LogEvent()), _filterConfig, _emailConfig);

            var result = target.CreateHtmlReport();

            Assert.That(result, Is.StringContaining(_filterConfig.MaxMessages -1 + " previously unreported events"));
        }

        [Test]
        public void ShouldIndicateMaxMessagesReached()
        {
            var target =
                new HtmlReportGenerator(
                    Enumerable.Range(0, _filterConfig.MaxMessages).Select(i => new LogEvent()),_filterConfig, _emailConfig);

            var result = target.CreateHtmlReport();

            Assert.That(result, Is.StringContaining(_filterConfig.MaxMessages+" latest previously unreported events"));
        }

        [Test]
        public void ShouldNotCrashIfNullPassedAsEvents()
        {
            var target = new HtmlReportGenerator(null, _filterConfig, _emailConfig);

            var result = target.CreateHtmlReport();

            Assert.That(result, Is.StringContaining("0 previously unreported events"));
        }
    }
}
