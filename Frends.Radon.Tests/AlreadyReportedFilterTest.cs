using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;

namespace Frends.Radon.Tests
{
    [TestFixture]
    public class AlreadyReportedFilterTest
    {
        private IEventReader _mockEventSource;
        private IEventIdentificationStore _mockEventIdentificationStore;
        private AlreadyReportedFilter _target;
        private IList<LogEvent> _events;

        [SetUp]
        public void Setup()
        {
            _mockEventSource = MockRepository.GenerateMock<IEventReader>();
            _mockEventIdentificationStore = MockRepository.GenerateMock<IEventIdentificationStore>();

            _events = Enumerable.Range(0, 10).Select(i => new LogEvent {Message = i.ToString(), TimeGenerated = DateTime.Now.AddHours(-i)}).ToList();
            _mockEventSource.Expect(e => e.ReadEvents()).Return(_events);

            _target = new AlreadyReportedFilter(_mockEventSource, _mockEventIdentificationStore);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(8)]
        [TestCase(9)]
        public void ReadEvents_ShouldUseOldEvent(int eventIndex)
        {
            _mockEventIdentificationStore.Expect(s => s.GetAlreadyReportedEventIdentification()).Return(HashBuilder.BuildEventIdentification(_events[eventIndex]));

            var result = _target.ReadEvents();

            Assert.That(result.Count(), Is.EqualTo(eventIndex)); // Events returned until the old event timestamp + hash encountered
        }

        [Test]
        public void ReadEvents_ShouldReturnAllIfNoOldEventFound()
        {
            _mockEventIdentificationStore.Expect(s => s.GetAlreadyReportedEventIdentification()).Return(null);

            Assert.That(_target.ReadEvents().Count(), Is.EqualTo(10)); // All events returned
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(8)]
        [TestCase(9)]
        public void ReadEvents_ShouldNotReturnOlderEventsEvenIfHashDoesNotMatch(int eventIndex)
        {
            _mockEventIdentificationStore.Expect(s => s.GetAlreadyReportedEventIdentification())
                                         .Return(new EventIdentification
                                         {
                                             Hash = "FOOBAR",
                                             TimeStampUtc = _events[eventIndex].TimeGenerated.ToUniversalTime()
                                         });

            var result = _target.ReadEvents();

            Assert.That(result.Count(), Is.EqualTo(eventIndex+1)); // Events returned until older event encountered
        }
    }
}
