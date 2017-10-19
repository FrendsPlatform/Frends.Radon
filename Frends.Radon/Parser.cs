using System;
using System.Collections.Generic;
using System.Linq;

namespace Frends.Radon
{
    public static class Parser
    {
        public static int TryParseInt(string numberValue, int defaultValue)
        {
            int tempValue;
            return int.TryParse(numberValue, out tempValue) ? tempValue : defaultValue;
        }

        public static bool TryParseBool(string boolValue, bool defaultValue)
        {
            bool tempValue;
            return bool.TryParse(boolValue, out tempValue) ? tempValue : defaultValue;
        }

        public static TimeSpan TryParseTimeSpan(string timespanValue, TimeSpan defaultValue)
        {
            TimeSpan tempValue;
            return TimeSpan.TryParse(timespanValue, out tempValue) ? tempValue : defaultValue;
        }

        public static List<string> ParseRecipientsToList(string recipients)
        {
            return string.IsNullOrEmpty(recipients) ? new List<string>() : recipients.Split(';').Select(m => m.Trim()).Where(u => !string.IsNullOrEmpty(u)).ToList();
        }

        public static string ParseRecipientsToString(IEnumerable<string> recipients)
        {
            return recipients == null ? string.Empty : string.Join(";", recipients.Where(u => !string.IsNullOrWhiteSpace(u)).Select(m => m.Trim()).ToArray());
        }

        public static string TrimLineOrReturnStringEmptyIfNull(string line)
        {
            return line==null ? string.Empty : line.Trim();
        }
    }
}
