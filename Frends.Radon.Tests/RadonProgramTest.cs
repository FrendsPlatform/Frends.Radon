using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;

namespace Frends.Radon.Tests
{
    [TestFixture]
    public class RadonProgramTest
    {
        private IRadonExecutor _mockRadonExecutor;
        private RadonProgram _radonProgram;

        [SetUp]
        public void Setup()
        {
            _mockRadonExecutor = MockRepository.GenerateMock<IRadonExecutor>();
            _radonProgram = new RadonProgram(_mockRadonExecutor);
        }
        
        [Test]
        public void ExecuteRadon_ShouldNotSendReportWhenThereAreNoEvents()
        {
            _mockRadonExecutor.Expect(m => m.GetLogEvents()).Return(new List<LogEvent>());

            var result = _radonProgram.ExecuteRadon();

            _mockRadonExecutor.AssertWasNotCalled(sender => sender.SendReport(Arg<IEnumerable<LogEvent>>.Is.Anything));
            _mockRadonExecutor.AssertWasNotCalled(sender => sender.SaveAlreadyReportedEventIdentification(Arg<LogEvent>.Is.Anything));

            Assert.That(result.UserResultMessage, Is.StringContaining("No events."));
            Assert.That(result.Success, Is.EqualTo(true));
            Assert.That(result.ActionSkipped, Is.EqualTo(true));
            Assert.That(result.UnreportedEventsCount, Is.EqualTo(0));
        }

        [Test]
        public void ExecuteRadon_ShouldSendReportWhenEventsFound()
        {
            _mockRadonExecutor.Expect(m => m.GetLogEvents()).Return(new List<LogEvent> { new LogEvent() });
            _mockRadonExecutor.Expect(m => m.EmailConfig).Return(new EmailConfiguration());

            var result = _radonProgram.ExecuteRadon();
            
            _mockRadonExecutor.AssertWasCalled(sender => sender.SendReport(Arg<IEnumerable<LogEvent>>.Is.Anything));
            _mockRadonExecutor.AssertWasCalled(sender => sender.SaveAlreadyReportedEventIdentification(Arg<LogEvent>.Is.Anything));

            Assert.That(result.UserResultMessage, Is.StringContaining("Mail sent to"));
            Assert.That(result.Success, Is.EqualTo(true));
            Assert.That(result.ActionSkipped, Is.EqualTo(false));
            Assert.That(result.UnreportedEventsCount, Is.EqualTo(1));
        }

        [Test]
        public void ExecuteRadon_ShouldNotSaveLastEventIfSendReportThrowsAnException()
        {
            _mockRadonExecutor.Expect(m => m.GetLogEvents()).Return(new List<LogEvent> { new LogEvent() });
            _mockRadonExecutor.Expect(m => m.SendReport(Arg<IEnumerable<LogEvent>>.Is.Anything)).Throw(new Exception("APUA!"));

            Assert.Throws<Exception>(() => _radonProgram.ExecuteRadon());

            _mockRadonExecutor.AssertWasCalled(sender => sender.SendReport(Arg<IEnumerable<LogEvent>>.Is.Anything));
            _mockRadonExecutor.AssertWasNotCalled(sender => sender.SaveAlreadyReportedEventIdentification(Arg<LogEvent>.Is.Anything));
        }
    }
}
