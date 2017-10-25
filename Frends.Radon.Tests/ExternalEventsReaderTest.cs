using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Is = NUnit.Framework.Is;

namespace Frends.Radon.Tests
{
    [TestFixture]
    public class ExternalEventsReaderTest
    {
        private FilterConfiguration CreateFilterConfig(string externalEventXml)
        {
            return new FilterConfiguration("", TimeSpan.FromDays(1), maxMessages: 1000, remoteMachine:string.Empty, eventLogName:string.Empty, eventSource: EventSource.External, externalEventsXml: externalEventXml);
        }

        [TestCase("")]
        [TestCase(null)]
        public void ReadEvents_ShouldReturnEmptlyListForEmptyOrNullExternalEventsXml(string xml)
        {
            var target = new ExternalEventsReader(CreateFilterConfig(xml));
            var events = target.ReadEvents();

            Assert.That(events.Count(), Is.EqualTo(0));
        }

        [Test]
        public void ReadEvents_ShouldParseEventXml()
        {
            var target = new ExternalEventsReader(CreateFilterConfig(@"<Events>
<Event>
    <Message>Performance counters for the WmiApRpl (WmiApRpl) service were loaded successfully. The Record Data in the data section contains the new index values assigned to this service.</Message>
    <Source>Microsoft-Windows-LoadPerf</Source>
    <TimeGenerated>2014-10-14 13:59:09Z</TimeGenerated>
    <Category>(0)</Category>
    <EventID>1000</EventID>
    <CategoryNumber>0</CategoryNumber>
    <Index>197340</Index>
    <ErrorLevel>0</ErrorLevel>
    <InstanceID>1000</InstanceID>
    <EntryType>Information</EntryType>
</Event>
<Event>
    <Message>The description for Event ID '1073742831' in Source 'Customer Experience Improvement Program' cannot be found.  The local computer may not have the necessary registry information or message DLL files to display the message, or you may not have permission to access them.  The following information is part of the event:</Message>
    <Source>Customer Experience Improvement Program</Source>
    <TimeGenerated>2014-10-14 13:50:08Z</TimeGenerated>
    <Category>(0)</Category>
    <EventID>1007</EventID>
    <CategoryNumber>0</CategoryNumber>
    <Index>197338</Index>
    <ErrorLevel>0</ErrorLevel>
    <InstanceID>1073742831</InstanceID>
    <EntryType>Information</EntryType>
</Event>
</Events>"));

            var events = target.ReadEvents();

            Assert.That(events.Count(), Is.EqualTo(2));
            var testEvent = events.Single(e => e.EventID == 1000);

            Assert.That(testEvent.Message, Is.EqualTo("Performance counters for the WmiApRpl (WmiApRpl) service were loaded successfully. The Record Data in the data section contains the new index values assigned to this service."));
            Assert.That(testEvent.Source, Is.EqualTo("Microsoft-Windows-LoadPerf"));
            Assert.That(testEvent.TimeGenerated, Is.InRange(DateTime.Parse("2014-10-14 13:59:09Z").ToUniversalTime(), DateTime.Parse("2014-10-14 13:59:09Z").ToUniversalTime()));
            Assert.That(testEvent.Category, Is.EqualTo("(0)"));
            Assert.That(testEvent.CategoryNumber, Is.EqualTo(0));
            Assert.That(testEvent.Index, Is.EqualTo(197340));
            Assert.That(testEvent.ErrorLevel, Is.EqualTo(0));
            Assert.That(testEvent.InstanceID, Is.EqualTo(1000));
            Assert.That(testEvent.EntryType, Is.EqualTo(EventLogEntryType.Information));
        }
    }
}
