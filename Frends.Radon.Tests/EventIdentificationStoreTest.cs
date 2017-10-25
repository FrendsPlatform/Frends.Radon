using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Frends.Radon.Tests
{
    [TestFixture]
    public class EventIdentificationStoreTest
    {
        private FilterConfiguration _filterConfig;
        private readonly LogEvent _testEvent = new LogEvent
        {
            Category = "Category",
            CategoryNumber = 42,
            EntryType = EventLogEntryType.Warning,
            ErrorLevel = 2,
            EventID = 24,
            Index = 0,
            InstanceID = 123,
            TimeGenerated = DateTime.Now
        };

        private Random _random;

        [SetUp]
        public void Setup()
        {
            _filterConfig = new FilterConfiguration("foo", TimeSpan.FromHours(1), 100, "foobar", "baz");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            EventIdentificationStore.ClearStoreFiles();
        }

        [Test]
        public void TestParallelAccess()
        {
            var threads = new List<Thread>();
            var handles = new List<WaitHandle>();
            for (int i = 0; i < 64; i++)
            {
                var handle = new AutoResetEvent(false);
                if (i%2 == 0)
                {
                    threads.Add(new Thread((ThreadStart)delegate
                    {
                        var store = new EventIdentificationStore(_filterConfig);

                        store.SaveAlreadyReportedEventIdentification(_testEvent);
                        handle.Set();
                    }));
                }
                else
                {
                    threads.Add(new Thread((ThreadStart)delegate
                    {
                        var store = new EventIdentificationStore(_filterConfig);

                        store.GetAlreadyReportedEventIdentification();
                        handle.Set();
                    }));
                }
                
                handles.Add(handle);
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            WaitHandle.WaitAll(handles.ToArray());
        }

        [Test]
        public void CanSaveAndLoadEventIdentifications()
        {
            var target = new EventIdentificationStore(_filterConfig);
            _random = new Random();
            // Generate some random content to make sure we use a unique hash
            _testEvent.InstanceID = _random.Next();
            _testEvent.Message = _random.NextDouble().ToString();

            target.SaveAlreadyReportedEventIdentification(_testEvent);

            var expectedIdentification = HashBuilder.BuildEventIdentification(_testEvent);

            var result = target.GetAlreadyReportedEventIdentification();

            Assert.That(result.Hash, Is.EqualTo(expectedIdentification.Hash));
            Assert.That(result.TimeStampUtc.ToString("u"), Is.EqualTo(expectedIdentification.TimeStampUtc.ToString("u")));
            Assert.That(result.TimeStampUtc.ToString("u"), Is.EqualTo(_testEvent.TimeGenerated.ToUniversalTime().ToString("u")));
        }

        [Test]
        public void FilterChangeShouldResetIdentification()
        {
            var oldStore = new EventIdentificationStore(_filterConfig);
            oldStore.SaveAlreadyReportedEventIdentification(_testEvent);

            var newConfig = new FilterConfiguration("newFilter", TimeSpan.FromHours(1), 100, "Foobar", "baz");
            var newStore = new EventIdentificationStore(newConfig);

            Assert.That(newStore.GetAlreadyReportedEventIdentification(), Is.EqualTo(null));
        }
    }
}
