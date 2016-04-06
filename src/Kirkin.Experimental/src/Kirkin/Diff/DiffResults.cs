using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kirkin.Diff
{
    internal sealed class SimpleDiffResult : IDiffResult
    {
        public bool AreSame { get; }
        public string Message { get; }

        internal SimpleDiffResult(bool areSame, string message)
        {
            AreSame = areSame;
            Message = message;
        }

        public override string ToString()
        {
            return Message;
        }
    }

    internal sealed class MultiDiffResult : IDiffResult
    {
        private readonly Lazy<IDiffResult[]> _entries;

        public IDiffResult[] Entries
        {
            get
            {
                return _entries.Value;
            }
        }

        public bool AreSame
        {
            get
            {
                if (Entries.Length != 0)
                {
                    foreach (IDiffResult entry in Entries)
                    {
                        if (!entry.AreSame) {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

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

        internal MultiDiffResult(Func<IEnumerable<IDiffResult>> diffFactory)
        {
            _entries = new Lazy<IDiffResult[]>(() => diffFactory().ToArray());
        }

        public override string ToString()
        {
            return Message;
        }
    }
}