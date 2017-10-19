using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Frends.Radon.Tests
{
    [TestFixture]
    public class EventLogFactoryTest
    {
        private EventLogFactory target = new EventLogFactory();

        private FilterConfiguration CreateConfig(string remote, string logName)
        {
            return new FilterConfiguration("", new TimeSpan(), 1, remote, logName);
        }

        [Test]
        public void CreateEventLog_ThrowsExceptionOnInvalidEventLog()
        {
            Assert.Throws(typeof (ArgumentException), () => target.CreateEventLog(CreateConfig("", "FooBarDoesNotExist")));
        }

        [Test]
        public void CreateEventLog_ThrowsExceptionOnInvalidMachine()
        {
            Assert.Throws(typeof(ArgumentException), () => target.CreateEventLog(CreateConfig("", "FooBarDoesNotExist")));
        }

        [Test]
        public void CreateEventLog_CreatesApplicationLogIfEventLogNameIsEmpty()
        {
            var config = CreateConfig("", "");
            var test = target.CreateEventLog(config);

            Assert.That(test.LogDisplayName, Is.EqualTo("Application"));
        }

        [Test]
        public void CreateEventLog_CreatesApplicationLogIfEventLogNameIsNull()
        {
            var config = CreateConfig("", null);
            var test = target.CreateEventLog(config);

            Assert.That(test.LogDisplayName, Is.EqualTo("Application"));
        }

        [Test]
        public void CreateEventLog_CreatesLocalLogIfRemoteMachineIsEmpty()
        {
            var config = CreateConfig("", null);
            var test = target.CreateEventLog(config);

            Assert.That(test.MachineName, Is.EqualTo(".")); //MachineName is . if the log is initialized without the machineName
        }

        [Test]
        public void CreateEventLog_CreatesLocalLogIfRemoteMachineIsNull()
        {
            var config = CreateConfig(null, null);
            var test = target.CreateEventLog(config);

            Assert.That(test.MachineName, Is.EqualTo(".")); //MachineName is . if the log is initialized without the machineName
        }

        [Test]
        public void CreateEventLog_CreatesRemoteLogIfRemoteMachineDefined()
        {
            var config = CreateConfig(Environment.MachineName, null);
            var test = target.CreateEventLog(config);

            Assert.That(test.MachineName, Is.EqualTo(Environment.MachineName));
        }

        [Test]
        public void CreateEventLog_CreatesSecurityLogIfEventLogNameSetToSecurityLog()
        {
            var config = CreateConfig(null, "Security");
            var test = target.CreateEventLog(config);

            Assert.That(test.LogDisplayName, Is.EqualTo("Security"));
        }
    }
}
