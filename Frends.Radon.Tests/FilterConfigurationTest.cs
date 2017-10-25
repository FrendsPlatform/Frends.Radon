using System;
using NUnit.Framework;

namespace Frends.Radon.Tests
{
    [TestFixture]
    public class FilterConfigurationTest
    {
        private string _filterString;
        private TimeSpan _timeLimit;
        private int _maxMessages;
        private string _remoteMachine;
        private string _eventLogName;
        
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            _filterString = "MyFilterString";
            _timeLimit = TimeSpan.FromHours(2);
            _maxMessages = 100;
            _remoteMachine = "MyRemoteMachine";
            _eventLogName = "MyEventLogName";
        }

        [Test]
        public void ShouldReturnCorrectValues()
        {
            var filterConfiguration = new FilterConfiguration(_filterString, _timeLimit, _maxMessages, _remoteMachine, _eventLogName);
            Assert.That(filterConfiguration.FilterString, Is.EqualTo(_filterString));
            Assert.That(filterConfiguration.TimeLimit, Is.EqualTo(_timeLimit));
            Assert.That(filterConfiguration.MaxMessages, Is.EqualTo(_maxMessages));
            Assert.That(filterConfiguration.UseMaxMessages, Is.True);
            Assert.That(filterConfiguration.RemoteMachine, Is.EqualTo(_remoteMachine));
            Assert.That(filterConfiguration.EventLogName, Is.EqualTo(_eventLogName));

            var otherConstructorFilterConfiguration = new FilterConfiguration(_filterString, _timeLimit, _maxMessages, _remoteMachine, _eventLogName);
            Assert.That(otherConstructorFilterConfiguration.FilterString, Is.EqualTo(_filterString));
            Assert.That(otherConstructorFilterConfiguration.TimeLimit, Is.EqualTo(_timeLimit));
            Assert.That(otherConstructorFilterConfiguration.MaxMessages, Is.EqualTo(_maxMessages));
            Assert.That(otherConstructorFilterConfiguration.UseMaxMessages, Is.True);
            Assert.That(otherConstructorFilterConfiguration.RemoteMachine, Is.EqualTo(_remoteMachine));
            Assert.That(otherConstructorFilterConfiguration.EventLogName, Is.EqualTo(_eventLogName));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ShouldNotUseMaxMessagesIfMaxMessagesIsLessThanOne(int maxMessages)
        {
            var filterConfiguration = new FilterConfiguration(_filterString, _timeLimit, maxMessages, _remoteMachine, _eventLogName);
            Assert.That(filterConfiguration.MaxMessages, Is.EqualTo(maxMessages));
            Assert.That(filterConfiguration.UseMaxMessages, Is.False);

            var otherConstructorFilterConfiguration = new FilterConfiguration(_filterString, _timeLimit, maxMessages, _remoteMachine, _eventLogName);
            Assert.That(otherConstructorFilterConfiguration.MaxMessages, Is.EqualTo(maxMessages));
            Assert.That(otherConstructorFilterConfiguration.UseMaxMessages, Is.False);
        }

        [Test]
        public void ShouldTrimRemoteMachineAndEventLogName()
        {
            const string remoteMachine = "      remoteMachine    ";
            const string eventLogName = "      eventLogName    ";

            var filterConfiguration = new FilterConfiguration(_filterString, _timeLimit, _maxMessages, remoteMachine, eventLogName);
            Assert.That(filterConfiguration.RemoteMachine, Is.EqualTo(remoteMachine.Trim()));
            Assert.That(filterConfiguration.EventLogName, Is.EqualTo(eventLogName.Trim()));

            var otherConstructorFilterConfiguration = new FilterConfiguration(_filterString, _timeLimit, _maxMessages, remoteMachine, eventLogName);
            Assert.That(otherConstructorFilterConfiguration.RemoteMachine, Is.EqualTo(remoteMachine.Trim()));
            Assert.That(otherConstructorFilterConfiguration.EventLogName, Is.EqualTo(eventLogName.Trim()));
        }

        [Test]
        public void ShouldNotTrimFilterString()
        {
            const string filterString = "      filterString    ";

            var filterConfiguration = new FilterConfiguration(filterString, _timeLimit, _maxMessages, _remoteMachine, _eventLogName);
            Assert.That(filterConfiguration.FilterString, Is.EqualTo(filterString));

            var otherConstructorFilterConfiguration = new FilterConfiguration(filterString, _timeLimit, _maxMessages, _remoteMachine, _eventLogName);
            Assert.That(otherConstructorFilterConfiguration.FilterString, Is.EqualTo(filterString));
        }

        [Test]
        public void ShouldReturnStringEmptyForFilterStringIfNullPassed()
        {
            var filterConfiguration = new FilterConfiguration(null, _timeLimit, _maxMessages, _remoteMachine, _eventLogName);
            Assert.That(filterConfiguration.FilterString, Is.EqualTo(string.Empty));

            var otherConstructorFilterConfiguration = new FilterConfiguration(null, _timeLimit, _maxMessages, _remoteMachine, _eventLogName);
            Assert.That(otherConstructorFilterConfiguration.FilterString, Is.EqualTo(string.Empty));
        }
    }
}
