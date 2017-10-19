using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;

namespace Frends.Radon
{
    public interface IEventIdentificationStore
    {
        EventIdentification GetAlreadyReportedEventIdentification();
        void SaveAlreadyReportedEventIdentification(LogEvent logEvent);
    }

    public class EventIdentificationStore : IEventIdentificationStore
    {
        private readonly IsolatedStorageFile _store;
        private readonly string _configurationHash;
        private static object _fileAccessLock = new object();

        public EventIdentificationStore(IFilterConfiguration configuration)
        {
            _store = GetIsolatedStorageFile();
            _configurationHash = HashBuilder.BuildFilterConfigHash(configuration);

           _store.CreateRadonDirectoryIfNotExists();
        }

        private static IsolatedStorageFile GetIsolatedStorageFile()
        {
            return IsolatedStorageFile.GetUserStoreForAssembly();
        }

        public EventIdentification GetAlreadyReportedEventIdentification()
        {
            lock (_fileAccessLock)
            {
                var fileNames = _store.GetRadonFileNames();
                if (fileNames.Any(f => f == _configurationHash))
                {

                    using (var stream = new IsolatedStorageFileStream(Path.Combine("Radon", _configurationHash),
                                                                   FileMode.Open, _store))
                    using (var reader = new StreamReader(stream))
                    {
                        return new EventIdentification
                        {
                            TimeStampUtc = DateTime.Parse(reader.ReadLine()).ToUniversalTime(),
                            Hash = reader.ReadLine()
                        };
                    }
                }
            }

            return null;
        }

        public void SaveAlreadyReportedEventIdentification(LogEvent logEvent)
        {
            lock (_fileAccessLock)
            {
                using (var stream = new IsolatedStorageFileStream(Path.Combine("Radon", _configurationHash), FileMode.Create, _store))
                using (var writer = new StreamWriter(stream))
                {
                    var id = HashBuilder.BuildEventIdentification(logEvent);
                    writer.WriteLine(id.TimeStampUtc.ToString("u"));
                    writer.WriteLine(id.Hash);
                }
            }
        }

        public static void ClearStoreFiles()
        {
            var store = GetIsolatedStorageFile();
            lock (_fileAccessLock)
            {
                var files = store.GetRadonFileNames();

                foreach (var file in files)
                {
                    store.DeleteFile(Path.Combine("Radon", file));
                }
            }
        }
    }

    public static class IsolatedStorageFileExtensions
    {
        public static IEnumerable<string> GetRadonFileNames(this IsolatedStorageFile store)
        {
            if (!RadonIsolatedStoreDirectoryExists(store))
            {
                return Enumerable.Empty<string>();
            }

            return store.GetFileNames("Radon\\*");
        }

        public static void CreateRadonDirectoryIfNotExists(this IsolatedStorageFile store)
        {
            if (!RadonIsolatedStoreDirectoryExists(store))
            {
                store.CreateDirectory("Radon");
            }
        }

        private static bool RadonIsolatedStoreDirectoryExists(IsolatedStorageFile store)
        {
            return store.GetDirectoryNames("Radon").Any(d => d == "Radon");
        }
    }
}