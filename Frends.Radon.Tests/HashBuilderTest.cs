using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Frends.Radon.Tests
{
    [TestFixture]
    public class HashBuilderTest
    {
        private LogEvent _logEvent;

        [SetUp]
        public void Setup()
        {
            _logEvent = new LogEvent { Category = "Category",CategoryNumber = 1, EntryType = EventLogEntryType.Information, ErrorLevel = 2, EventID = 3, Index = 4, InstanceID = 5, Message = "Test", Source = "Source", TimeGenerated = DateTime.Now};
        }
        [Test]
        public void BuildFilterConfigHash_ShouldGenerateSameHash()
        {
            var config = new FilterConfiguration("filter", TimeSpan.FromHours(1), 100, "foobar", "bazqux");

            Assert.That(HashBuilder.BuildFilterConfigHash(config), Is.EqualTo(HashBuilder.BuildFilterConfigHash(config)));
        }

        [Test]
        public void BuildEventIdentification_ShouldGenerateSameHash()
        {
            Assert.That(HashBuilder.BuildEventIdentification(_logEvent).Hash, Is.EqualTo(HashBuilder.BuildEventIdentification(_logEvent).Hash));
        }

        [Test]
        public void BuildEventIdentification_ShouldUseEventTimeGenerated()
        {
            Assert.That(HashBuilder.BuildEventIdentification(_logEvent).TimeStampUtc, Is.EqualTo(_logEvent.TimeGenerated.ToUniversalTime()));
        }

        [Test]
        public void BuildEventIdentification_ShouldNotBeAffectedByIndex()
        {
            var hash1 = HashBuilder.BuildEventIdentification(_logEvent).Hash;
            _logEvent.Index = 123;
            var hash2 = HashBuilder.BuildEventIdentification(_logEvent).Hash;

            Assert.That(hash1, Is.EqualTo(hash2));
        }

        private void AssertHashNotSameAfterFunction(Action change)
        {
            var hash1 = HashBuilder.BuildEventIdentification(_logEvent).Hash;

            change.Invoke();

            var hash2 = HashBuilder.BuildEventIdentification(_logEvent).Hash;

            Assert.That(hash1, Is.Not.EqualTo(hash2));
        }

        [Test]
        public void BuildEventIdentification_ShouldBeAffectedByCategory()
        {
            AssertHashNotSameAfterFunction(() => _logEvent.Category = "NewCategory");
        }

        [Test]
        public void BuildEventIdentification_ShouldBeAffectedByCategoryNumber()
        {
            AssertHashNotSameAfterFunction(() => _logEvent.CategoryNumber = 123);
        }

        [Test]
        public void BuildEventIdentification_ShouldBeAffectedByEntryType()
        {
            AssertHashNotSameAfterFunction(() => _logEvent.EntryType = EventLogEntryType.Warning);
        }

        [Test]
        public void BuildEventIdentification_ShouldBeAffectedByErrorLevel()
        {
            AssertHashNotSameAfterFunction(() => _logEvent.ErrorLevel = 123);
        }

        [Test]
        public void BuildEventIdentification_ShouldBeAffectedByEventID()
        {
            AssertHashNotSameAfterFunction(() => _logEvent.EventID = 123);
        }

        [Test]
        public void BuildEventIdentification_ShouldBeAffectedByInstanceId()
        {
            AssertHashNotSameAfterFunction(() => _logEvent.InstanceID = 123);
        }

        [Test]
        public void BuildEventIdentification_ShouldBeAffectedByMessage()
        {
            AssertHashNotSameAfterFunction(() => _logEvent.Message = "NewMessage");
        }

        [Test]
        public void BuildEventIdentification_ShouldBeAffectedBySource()
        {
            AssertHashNotSameAfterFunction(() => _logEvent.Source = "NewSource");
        }

        [Test]
        public void BuildEventIdentification_ShouldBeAffectedByTimeGenerated()
        {
            AssertHashNotSameAfterFunction(() => _logEvent.TimeGenerated = DateTime.Now.AddHours(1));
        }

        [Test]
        public void BuildFilterConfigHash_ShouldBeAffectedByFilterString()
        {
            var original = new FilterConfiguration("filter", TimeSpan.FromHours(1), 100, "foobar", "bazqux");
            var newConfig = new FilterConfiguration("newFilter", TimeSpan.FromHours(1), 100, "foobar", "bazqux");

            Assert.That(HashBuilder.BuildFilterConfigHash(original), Is.Not.EqualTo(HashBuilder.BuildFilterConfigHash(newConfig)));
        }

        [Test]
        public void BuildFilterConfigHash_ShouldBeAffectedByTimeLimit()
        {
            var original = new FilterConfiguration("filter", TimeSpan.FromHours(1), 100, "foobar", "bazqux");
            var newConfig = new FilterConfiguration("filter", TimeSpan.FromHours(2), 100, "foobar", "bazqux");

            Assert.That(HashBuilder.BuildFilterConfigHash(original), Is.Not.EqualTo(HashBuilder.BuildFilterConfigHash(newConfig)));
        }

        [Test]
        public void BuildFilterConfigHash_ShouldBeAffectedByMaxCount()
        {
            var original = new FilterConfiguration("filter", TimeSpan.FromHours(1), 100, "foobar", "bazqux");
            var newConfig = new FilterConfiguration("filter", TimeSpan.FromHours(1), 1000, "foobar", "bazqux");

            Assert.That(HashBuilder.BuildFilterConfigHash(original), Is.Not.EqualTo(HashBuilder.BuildFilterConfigHash(newConfig)));
        }

        [Test]
        public void BuildFilterConfigHash_ShouldBeAffectedByRemoteMachine()
        {
            var original = new FilterConfiguration("filter", TimeSpan.FromHours(1), 100, "foobar", "bazqux");
            var newConfig = new FilterConfiguration("filter", TimeSpan.FromHours(1), 100, "bazqux", "bazqux");

            Assert.That(HashBuilder.BuildFilterConfigHash(original), Is.Not.EqualTo(HashBuilder.BuildFilterConfigHash(newConfig)));
        }

        [Test]
        public void BuildFilterConfigHash_ShouldBeAffectedByEventLogName()
        {
            var original = new FilterConfiguration("filter", TimeSpan.FromHours(1), 100, "foobar", "bazqux");
            var newConfig = new FilterConfiguration("filter", TimeSpan.FromHours(1), 100, "foobar", "foobar");

            Assert.That(HashBuilder.BuildFilterConfigHash(original), Is.Not.EqualTo(HashBuilder.BuildFilterConfigHash(newConfig)));
        }
    }
}
