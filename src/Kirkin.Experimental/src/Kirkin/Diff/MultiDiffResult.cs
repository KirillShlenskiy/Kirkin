using System.Collections.Generic;
using System.Linq;

namespace Kirkin.Diff
{
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
                return "";
            }
        }

        internal MultiDiffResult(string name, IEnumerable<IDiffResult> entries)
        {
            Name = name;
            _entries = entries.ToArray();
        }

        public override string ToString()
        {
            return Message;
        }
    }
}