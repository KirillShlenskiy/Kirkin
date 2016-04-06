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

    public abstract class CompositeDiffResult<T> : IDiffResult
    {
        public T X { get; }
        public T Y { get; }

        private IDiffResult[] _entries;

        public IDiffResult[] Entries
        {
            get
            {
                if (_entries == null) {
                    _entries = Compare(X, Y).ToArray();
                }

                return _entries;
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

        protected CompositeDiffResult(T x, T y)
        {
            X = x;
            Y = y;
        }

        protected abstract IEnumerable<IDiffResult> Compare(T x, T y);

        public override string ToString()
        {
            return Message;
        }
    }
}