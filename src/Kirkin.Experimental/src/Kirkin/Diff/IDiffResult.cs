using System.Collections.Generic;

namespace Kirkin.Diff
{
    public interface IDiffResult
    {
        IEnumerable<IDiffResult> Entries { get; }

        bool AreSame { get; }
        string Name { get; }
        string Message { get; }
    }
}