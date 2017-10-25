using System.Text.RegularExpressions;

namespace Frends.Radon.LiquidExtensions
{
    public static class FilterExtensions
    {
        public static string MatchFirst(string input)
        {
            return MatchFirst(input, "");
        }

        public static string MatchFirst(string input, string regexPattern)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(regexPattern))
            {
                return "";
            }

            var regex = new Regex(regexPattern);

            var result = regex.Match(input);

            if (result.Success)
            {
                if (result.Groups.Count >= 2) // when there's more than one element, the capture group was matched
                {
                    return result.Groups[1].Value;
                }
                else
                {
                    return result.Value;
                }
            }

            return "";
        }
    }
}
