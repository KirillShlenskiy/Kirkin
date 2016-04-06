using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kirkin.Diff
{
    internal static class MessageBuilder
    {
        public static string BuildMessage(IDiffResult diffResult)
        {
            StringBuilder sb = new StringBuilder();

            BuildMessage(sb, 0, diffResult);

            return sb.ToString();
        }

        private static void BuildMessage(StringBuilder sb, int indenting, IDiffResult diffResult)
        {
            if (!diffResult.AreSame)
            {
                if (indenting != 0) {
                    sb.Append(new string(' ', indenting * 4));
                }

                sb.Append(diffResult.Name);
                sb.Append(": ");
                sb.Append(diffResult.Message);

                foreach (IDiffResult childEntry in diffResult.Entries) {
                    BuildMessage(sb, indenting + 1, childEntry);
                }
            }
        }
    }

    internal sealed class SimpleDiffResult : IDiffResult
    {
        public IEnumerable<IDiffResult> Entries { get; } = Enumerable.Empty<IDiffResult>();
        public bool AreSame { get; }
        public string Name { get; }
        public string Message { get; }

        internal SimpleDiffResult(string name, bool areSame, string message)
        {
            Name = name;
            AreSame = areSame;
            Message = message;
        }

        public override string ToString()
        {
            return MessageBuilder.BuildMessage(this);
        }
    }

    internal sealed class MultiDiffResult : IDiffResult
    {
        private readonly IDiffResult[] _entries;

        public IEnumerable<IDiffResult> Entries
        {
            get
            {
                return _entries;
            }
        }

        public bool AreSame
        {
            get
            {
                foreach (IDiffResult entry in Entries)
                {
                    if (!entry.AreSame) {
                        return false;
                    }
                }

                return true;
            }
        }

        public string Name { get; }

        public string Message
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                foreach (IDiffResult entry in Entries)
                {
                    if (!entry.AreSame) {
                        sb.AppendLine(entry.Message);
                    }
                }

                return sb.ToString();
            }
        }

        internal MultiDiffResult(string name, IEnumerable<IDiffResult> entries)
        {
            Name = name;
            _entries = entries.ToArray();
        }

        public override string ToString()
        {
            return MessageBuilder.BuildMessage(this);
        }
    }
}