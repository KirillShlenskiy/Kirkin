using System.Collections.Generic;
using System.Linq;

namespace Kirkin.Diff
{
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
            return Message;
        }
    }
}