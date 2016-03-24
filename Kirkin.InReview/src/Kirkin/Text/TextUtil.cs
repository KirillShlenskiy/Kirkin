using System;
using System.Text;

namespace Kirkin.Text
{
    /// <summary>
    /// Common text methods.
    /// </summary>
    public static class TextUtil
    {
        /// <summary>
        /// Returns the given string with the first
        /// character and every character following
        /// a non-letter character converted to
        /// uppercase, with the rest of the characters
        /// converted to lowercase if necessary.
        /// Does not work well for acronyms.
        /// </summary>
        public static string CapitaliseFirstLetterOfEachWord(string text)
        {
            if (text == null) throw new ArgumentNullException("text");
            if (text.Length == 0) return text;

            var prevChar = default(char);
            var chars = new char[text.Length];
            var i = 0;

            foreach (var c in text)
            {
                chars[i++] = char.IsLetter(prevChar) ? char.ToLowerInvariant(c) : char.ToUpperInvariant(c);
                prevChar = c;
            }

            return new string(chars);
        }

        /// <summary>
        /// Replaces all occurrences of a singular Cr or Lf with CrLf.
        /// </summary>
        /// <remarks>
        /// Written by Jon Skeet for a Stack Overflow post, found here:
        /// http://stackoverflow.com/questions/841396/what-is-a-quick-way-to-force-crlf-in-c-sharp-net
        /// </remarks>
        public static string NormaliseLineBreaks(string input)
        {
            // Allow 10% as a rough guess of how much the string may grow.
            // If we're wrong we'll either waste space or have extra copies -
            // it will still work.
            var sb = new StringBuilder((int)(input.Length * 1.1));
            bool lastWasCR = false;

            foreach (char c in input)
            {
                if (lastWasCR)
                {
                    lastWasCR = false;

                    if (c == '\n') {
                        continue; // Already written \r\n
                    }
                }
                switch (c)
                {
                    case '\r':
                        sb.Append("\r\n");
                        lastWasCR = true;
                        break;
                    case '\n':
                        sb.Append("\r\n");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Removes occurrences of the given text inside the input string.
        /// </summary>
        public static string RemoveText(string input, string textToRemove, StringComparison comparisonType)
        {
            while (true)
            {
                int index = input.IndexOf(textToRemove, comparisonType);

                if (index == -1) {
                    return input;
                }

                input = input.Remove(index, textToRemove.Length);
            }
        }

        /// <summary>
        /// Returns a new string instance with a space preceding
        /// every capital letter except for the one at zero index.
        /// </summary>
        public static string SplitOnCaps(string input)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in input)
            {
                if (char.IsUpper(c) && sb.Length != 0) {
                    sb.Append(' ');
                }

                sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Joins the non empty strings with the given separator between them.
        /// </summary>
        public static string JoinNonEmpty(string separator, params string[] values)
        {
            var sb = new StringBuilder();

            foreach (var v in values)
            {
                if (!string.IsNullOrEmpty(v))
                {
                    if (sb.Length != 0) {
                        sb.Append(separator);
                    }

                    sb.Append(v);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns the suffix appropriate for the given number.
        /// </summary>
        public static string NumericSuffix(int position)
        {
            if (position > 10 && position < 20) return "th";
            if (position % 10 == 1) return "st";
            if (position % 10 == 2) return "nd";
            if (position % 10 == 3) return "rd";

            return "th";
        }
    }
}