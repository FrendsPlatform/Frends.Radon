using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Frends.Radon.Tests
{
    [TestFixture]
    public class EventReaderTest
    {
        private EventLog _testEventLog;

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            if (!EventLog.SourceExists("RadonTestEventSource"))
                EventLog.CreateEventSource("RadonTestEventSource", "RadonTestLog");
            _testEventLog = new EventLog("RadonTestLog") {Source = "RadonTestEventSource"};
        }

        [SetUp]
        public void Setup()
        {
            _testEventLog.Clear();
        }

        [Test]
        public void ShouldReadAllItemsFromGivenLog()
        {
            SetupEvents(1000, EventLogEntryType.Error);
            SetupEvents(1000, EventLogEntryType.Warning);
            SetupEvents(1000, EventLogEntryType.Information);
            SetupEvents(100, EventLogEntryType.SuccessAudit);
            SetupEvents(100, EventLogEntryType.FailureAudit);

            EventReader reader = new EventReader(_testEventLog, TimeSpan.FromHours(1));

            List<LogEvent> events = new List<LogEvent>(reader.ReadEvents());
            
            Assert.That(events.Count(), Is.EqualTo(3200));
        }

        private void SetupEvents(int count, EventLogEntryType level)
        {
            for (int i = 0;  i < count; i++)
            {
                _testEventLog.WriteEntry("TestData", level);                
            }            
        }


        [Test]
        public void ShouldReadAllFieldsFromEvent()
        {
            const int eventId = 32456;            
            const int categoryId = 32;            
            const string message = "An error occurred!!!";
            _testEventLog.WriteEntry(message, EventLogEntryType.Error, eventId, categoryId);

            var reader = new EventReader(_testEventLog, TimeSpan.FromMinutes(1));
            var events = reader.ReadEvents();

            Assert.That(events.Count(), Is.EqualTo(1));
            var firstEvent = events.First();
            Assert.That(firstEvent.ErrorLevel, Is.EqualTo(2)); // == EventLogEntryType.Error
            Assert.That(firstEvent.EventID, Is.EqualTo(eventId));
            Assert.That(firstEvent.CategoryNumber, Is.EqualTo(categoryId));
            Assert.That(firstEvent.Message, Is.EqualTo(message));
            Assert.That(firstEvent.EntryType, Is.EqualTo(EventLogEntryType.Error));
            Assert.That(firstEvent.InstanceID, Is.EqualTo(eventId));
            Assert.That(firstEvent.Source, Is.EqualTo(_testEventLog.Source));
            Assert.That(firstEvent.TimeGenerated, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(10)));

        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            EventLog.DeleteEventSource("RadonTestEventSource");
            
            if (EventLog.Exists("RadonTestLog"))
                EventLog.Delete("RadonTestLog");
            

        }
    }
}
