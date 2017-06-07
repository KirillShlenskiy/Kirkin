using System.Collections.Generic;
using System.Text;

namespace Kirkin.CommandLine
{
    internal sealed class TextFormatter
    {
        internal static string FormatAsTable(Dictionary<string, string> dictionary)
        {
            const int screenWidth = 72;
            int maxCommandWidth = 0;

            foreach (string key in dictionary.Keys)
            {
                if (key.Length > maxCommandWidth) {
                    maxCommandWidth = key.Length;
                }
            }

            StringBuilder sb = new StringBuilder();
            const string tab = "    ";
            int leftColumnWidth = tab.Length * 2 + maxCommandWidth;

            foreach (KeyValuePair<string, string> kvp in dictionary)
            {
                sb.Append(tab);
                sb.Append(kvp.Key.PadRight(maxCommandWidth));
                sb.Append(tab);

                if (string.IsNullOrEmpty(kvp.Value)) {
                    continue;
                }

                int position = leftColumnWidth;

                foreach (string word in kvp.Value.Split(' '))
                {
                    if (position + word.Length + 1 /* space */ > screenWidth)
                    {
                        sb.AppendLine();
                        sb.Append(' ', leftColumnWidth);

                        position = leftColumnWidth;
                    }
                    else if (position > leftColumnWidth)
                    {
                        sb.Append(' ');

                        position++;
                    }

                    sb.Append(word);

                    position += word.Length;
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}