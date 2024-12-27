using System;
using System.Linq;

namespace Visometry.VisionLib.SDK.Core.Details
{
    public static class StringHelper
    {
        public static string GetIndefiniteArticle(string subject)
        {
            return IsVowel(subject[0]) ? "an" : "a";
        }

        public static bool IsVowel(char letter)
        {
            return "aeiouAEIOU".Contains(letter);
        }

        public static bool ParseAsBoolOrThrow(string value)
        {
            if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            throw new ArgumentException(
                $"The string '{value}' is either not a bool or contains additional characters.");
        }

        public static string Enquote(this string value)
        {
            return $"\"{value}\"";
        }

        public static int NewlineCount(this string text)
        {
            return text.Count(c => c == '\n') + 1;
        }
    }
}
