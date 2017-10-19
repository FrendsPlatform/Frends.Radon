using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;

namespace Frends.Radon.Tests
{
    [TestFixture]
    public class LogEventFilterTest
    {
        private static IEnumerable<LogEvent> GetEventsForTheLastHourWith(IEventReader reader, string filterString, int maxMessages)
        {
            return new LogEventFilter(reader, new FilterConfiguration(filterString, TimeSpan.FromHours(1), maxMessages, null, null)).ReadEvents();
        }


        [TestFixture]
        public class FaultyInputTest
        {
            private readonly IEventReader _reader = MockRepository.GenerateMock<IEventReader>();

            [SetUp]
            public void SetUp()
            {
                _reader.Expect(r => r.ReadEvents()).Return(new List<LogEvent>());
            }

            [Test]
            public void ShouldThrowParseExceptionForUnknownProperty()
            {
                try
                {
                    new LogEventFilter(_reader, new FilterConfiguration("UnknownProperty = 10", TimeSpan.FromHours(1), 100, null, null)).ReadEvents();

                    Assert.Fail("ParseException should have been thrown");
                }
                catch (ArgumentException ae)
                {
                    Assert.That(ae.Message, Is.StringContaining("UnknownProperty = 10"));
                    Assert.That(ae.ToString(), Is.StringContaining("UnknownProperty") & Is.StringContaining("at index 0"));
                }
            }

            [Test]
            public void ShouldThrowParseExceptionForInvalidCode()
            {
                const string filterString = "System.Diagnostics.Trace.TraceLine(\"whoppee!\");";
                try
                {
                    new LogEventFilter(_reader, new FilterConfiguration(filterString, TimeSpan.FromHours(1), 100, null, null)).ReadEvents();

                    Assert.Fail("ParseException should have been thrown");
                }
                catch (ArgumentException ae)
                {
                    Assert.That(ae.Message, Is.StringContaining(filterString));
                    Assert.That(ae.ToString(), Is.StringContaining("at index 0"));
                }
            }
        }
        [TestFixture]
        public class FiltersTest
        {
            private IEventReader _readerWithWith300Events;

            [SetUp]
            public void SetUp()
            {
                _readerWithWith300Events = MockRepository.GenerateMock<IEventReader>();
                var events = new List<LogEvent>();
                for (short i = 0; i < 100; i++)
                {
                    events.Add(new LogEvent{EventID = i, EntryType = EventLogEntryType.Information, Message = "info"+i, ErrorLevel = 0, Source="InfoSource"});
                    events.Add(new LogEvent { EventID = i, EntryType = EventLogEntryType.Warning, Message = "warn" + i, ErrorLevel = 1, Source="WarningSource" });
                    events.Add(new LogEvent { EventID = i, EntryType = EventLogEntryType.Error, Message = "error" + i, ErrorLevel = 2, Source="ErrorSource"});
                }

                _readerWithWith300Events.Expect(r => r.ReadEvents()).Return(events);
            }

            [Test]
            public void ShouldReturnAllEventsWhenEmptyFilter()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "", 400);

                Assert.That(resultEvents.Count(), Is.EqualTo(300));
            }

            [Test]
            public void ShouldLimitEventsToMaxEvents()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "", 20);
             
                Assert.That(resultEvents.Count(), Is.EqualTo(20));
            }

            [Test]
            public void ShouldLimitOnMinimumLevel()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "ErrorLevel > 0", 400);               

                Assert.That(resultEvents.Count(), Is.EqualTo(200));
            }

            [Test]
            public void ShouldLimitOnMaxLevel()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "ErrorLevel < 2", 400);

                Assert.That(resultEvents.Count(), Is.EqualTo(200));
            }

            [Test]
            public void ShouldLimitOnNotEqualLevel()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "ErrorLevel <> 2", 400);

                Assert.That(resultEvents.Count(), Is.EqualTo(200));
            }

            [Test]
            public void ShouldFilterOnSimpleEventIdFilterString()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "EventID = 32", 100);

                Assert.That(resultEvents.Count(), Is.EqualTo(3));
            }

            [Test]
            public void ShouldFilterOnSimpleSourceFilterString()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "Source = \"InfoSource\"", 500);

                Assert.That(resultEvents.Count(), Is.EqualTo(100));            
            }
            [Test]
            public void ShouldFilterOnNegatedSourceFilterString()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "Source <> \"InfoSource\"", 500);

                Assert.That(resultEvents.Count(), Is.EqualTo(200));
            }

            [Test]
            public void AndShouldCombineFilters()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "EventID = 32 And EntryType = \"Information\"", 100);

                Assert.That(resultEvents.Count(), Is.EqualTo(1));                
            }

            [Test]
            public void UnknownIdQueryShouldReturnEmptyList()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "EventID = 10000", 100);

                Assert.That(resultEvents.Count(), Is.EqualTo(0));   
            }

            [Test]
            public void OrShouldCombineFilters()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "EventID = 32 And EntryType = \"Information\" Or EventID = 44", 100);

                Assert.That(resultEvents.Count(), Is.EqualTo(4));               
            }

            [Test]
            public void ShouldAcceptNotEqual()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "EntryType <> \"Information\"", 400);

                Assert.That(resultEvents.Count(), Is.EqualTo(200));
            }

            [Test]
            public void ShouldAcceptExclamationEqualAsNotEqual()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "EntryType != \"Information\"", 400);

                Assert.That(resultEvents.Count(), Is.EqualTo(200));
            }

            [Test]
            public void ParenthesesShouldControlOperatorPrecedenceWithOr()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "(EventID = 32 And EntryType = \"Information\") Or EventID = 44", 100);

                Assert.That(resultEvents.Count(), Is.EqualTo(4)); 
            }

            [Test]
            public void ParenthesesShouldControlOperatorPrecedenceWithAnd()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "EventID = 32 And (EntryType = \"Information\" Or EventID = 44)", 100);

                Assert.That(resultEvents.Count(), Is.EqualTo(1)); 
            }

            [Test]
            public void NotShouldNegateResults()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "EventID != 32 And EntryType = \"Information\"", 100);

                Assert.That(resultEvents.Count(), Is.EqualTo(99)); 
            }

            [Test]
            public void RangeShouldReturnValues()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "EventID >= 10 And EventID < 30 And EntryType = \"Information\"", 100);

                Assert.That(resultEvents.Count(), Is.EqualTo(20));
            }

            [Test]
            public void MessageContentQueryShouldReturnMatchingContentValue()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "Message.Contains(\"warn\")", 200);

                Assert.That(resultEvents.Count(), Is.EqualTo(100));
            }

            [Test]
            public void MessageContentQueryShouldReturnNegativeMatchingContentValueWithExclamationMark()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "!Message.Contains(\"warn\")", 500);

                Assert.That(resultEvents.Count(), Is.EqualTo(200));
            }

            [Test]
            public void MessageContentQueryShouldReturnNegativeMatchingContentValueWithNot()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "Not Message.Contains(\"warn\")", 500);

                Assert.That(resultEvents.Count(), Is.EqualTo(200));
            }


            [Test]
            public void MessageContentQueryShouldReturnMatchingStartContentValue()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "Message.StartsWith(\"warn\")", 200);

                Assert.That(resultEvents.Count(), Is.EqualTo(100));
            }

            [Test]
            public void MessageContentQueryShouldReturnMatchingEndContentValue()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "Message.EndsWith(\"2\")", 200);
                               
                Assert.That(resultEvents.Count(), Is.EqualTo(3*10));
            }

            [Test]
            public void MessageContentQueryShouldReturnEmptyListIfNoMatchingContentValue()
            {
                var resultEvents = GetEventsForTheLastHourWith(_readerWithWith300Events, "Message.Contains(\"foo\")", 200);

                Assert.That(resultEvents.Count(), Is.EqualTo(0));
            }
        }
    }
}
