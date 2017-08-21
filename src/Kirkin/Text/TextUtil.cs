using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Kirkin.Text
{
    /// <summary>
    /// Common text methods.
    /// </summary>
    public static class TextUtil
    {
        /// <summary>
        /// Returns the given string with the first character and every character following
        /// a non-letter character converted to uppercase, with the rest of the characters
        /// converted to lowercase if necessary. Does not work well for acronyms.
        /// </summary>
        public unsafe static string CapitalizeFirstLetterOfEachWord(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (text.Length == 0) return text;

            char prevChar = default(char);
            char[] chars = new char[text.Length];
            int i = 0;

            foreach (char c in text)
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
        public static string NormalizeLineBreaks(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            // Allow 10% as a rough guess of how much the string may grow.
            // If we're wrong we'll either waste space or have extra copies -
            // it will still work.
            StringBuilder sb = new StringBuilder((int)(input.Length * 1.1));
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
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (textToRemove == null) throw new ArgumentNullException(nameof(textToRemove));

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
            if (input == null) throw new ArgumentNullException(nameof(input));

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
    }
}