using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Frends.Radon
{
    public static class HashBuilder
    {
        public static string BuildFilterConfigHash(IFilterConfiguration config)
        {
            // FilterConfigHash is used as a filename so we cannot use Base64, instead use BitConverter and remove the separating slashes
            return BitConverter.ToString(new SHA256Managed().ComputeHash(GetFilterConfigurationBytes(config))).Replace("-", String.Empty);
        }

        public static EventIdentification BuildEventIdentification(LogEvent logEvent)
        {
            return new EventIdentification
            {
                Hash = Convert.ToBase64String(new SHA256Managed().ComputeHash(GetEventBytes(logEvent))),
                TimeStampUtc = logEvent.TimeGenerated.ToUniversalTime()
            };
        }

        private static byte[] GetFilterConfigurationBytes(IFilterConfiguration config)
        {
            return
                Encoding.UTF8.GetBytes(String.Join("\n", new[]
                {
                    config.EventLogName, 
                    config.FilterString, 
                    config.MaxMessages.ToString(CultureInfo.InvariantCulture),
                    config.RemoteMachine, 
                    config.TimeLimit.ToString(), 
                    config.UseMaxMessages.ToString()
                }));
        }

        private static byte[] GetEventBytes(LogEvent logEvent)
        {
            var logEventSerialized = String.Join("\n", new[]
                                                       {
                                                           logEvent.Category,
                                                           logEvent.CategoryNumber.ToString(CultureInfo.InvariantCulture), 
                                                           logEvent.EntryType.ToString(), 
                                                           logEvent.ErrorLevel.ToString(CultureInfo.InvariantCulture), 
                                                           logEvent.EventID.ToString(CultureInfo.InvariantCulture), 
                                                           logEvent.InstanceID.ToString(CultureInfo.InvariantCulture), 
                                                           logEvent.Message, 
                                                           logEvent.Source, 
                                                           logEvent.TimeGenerated.ToString(CultureInfo.InvariantCulture)
                                                       });
            var encodedSerializedEvent = Encoding.UTF8.GetBytes(logEventSerialized);
            return encodedSerializedEvent;
        }
    }
}