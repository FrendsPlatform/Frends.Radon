using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;

namespace Frends.Radon.Tests
{
    public class RadonExecutorTest
    {
        private IReportSender _mockReportSender;
        private IEmailConfiguration _mockEmailConfiguration;
        private IFilterConfiguration _mockFilterConfiguration;
        private IEventIdentificationStore _mockEventIdentificationStore;
        private IEventReader _mockEventReader;
        private IRadonExecutor _radonExecutor;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _mockReportSender = MockRepository.GenerateMock<IReportSender>();
            _mockEmailConfiguration = MockRepository.GenerateMock<IEmailConfiguration>();
            _mockFilterConfiguration = MockRepository.GenerateMock<IFilterConfiguration>();
            _mockEventIdentificationStore = MockRepository.GenerateMock<IEventIdentificationStore>();
            _mockEventReader = MockRepository.GenerateMock<IEventReader>();

            _radonExecutor = new RadonExecutor(_mockEmailConfiguration, _mockFilterConfiguration, _mockEventIdentificationStore, _mockEventReader);
        }

        [Test]
        public void GetLogEvents_ShouldCallReadEventsAndReturnThem()
        {
            var myEvents = new List<LogEvent> {new LogEvent { Message = "oho!!" }, new LogEvent{ Message = "vau!" }};

            _mockEventReader.Expect(m => m.ReadEvents()).Return(myEvents);

            var events = _radonExecutor.GetLogEvents();

            _mockEventReader.AssertWasCalled(m => m.ReadEvents());

            Assert.That(events.Count, Is.EqualTo(2));
            Assert.That(events[0].Message, Is.EqualTo("oho!!"));
            Assert.That(events[1].Message, Is.EqualTo("vau!"));
        }

        [Test]
        public void SaveAlreadyReportedEventIdentification_ShouldCallSaveAlreadyReportedEventIdentificationWithEvent()
        {
            var myEvent = new LogEvent { Message = "myMessage" };
            _radonExecutor.SaveAlreadyReportedEventIdentification(myEvent);

            _mockEventIdentificationStore.AssertWasCalled(m => m.SaveAlreadyReportedEventIdentification(Arg<LogEvent>.Is.Equal(myEvent)));
        }

        [Test]
        public void SendReport_ShouldSendEventsInReport()
        {            
            var myEvents = new List<LogEvent> { new LogEvent { Message = "oho!!" }, new LogEvent { Message = "vau!" } };

            _mockEmailConfiguration.Expect(m => m.GetReportSender()).Return(_mockReportSender);

            _radonExecutor.SendReport(myEvents);

            _mockEmailConfiguration.AssertWasCalled(m => m.GetReportSender());
            _mockReportSender.AssertWasCalled(sender => sender.SendReport(Arg<string>.Matches(m => m.Contains("2 latest previously unreported events")), Arg<string>.Is.Anything));
            _mockReportSender.AssertWasCalled(sender => sender.SendReport(Arg<string>.Matches(m => m.Contains("oho!!")), Arg<string>.Is.Anything));
            _mockReportSender.AssertWasCalled(sender => sender.SendReport(Arg<string>.Matches(m => m.Contains("vau!")), Arg<string>.Is.Anything));
        }
    }
}
