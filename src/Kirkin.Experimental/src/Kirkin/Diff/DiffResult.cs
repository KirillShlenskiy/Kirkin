using System.Collections.Generic;
using System.Linq;

namespace Kirkin.Diff
{
    /// <summary>
    /// Represents the result of a diff operation.
    /// </summary>
    public sealed class DiffResult
    {
        private static readonly DiffResult[] s_emptyEntries = new DiffResult[0];
        private DiffResult[] _entries;

        /// <summary>
        /// Child diff entries.
        /// </summary>
        public IReadOnlyList<DiffResult> Entries
        {
            get
            {
                return _entries;
            }
        }

        public bool AreSame { get; }

        /// <summary>
        /// Name of the comparison.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Diff message or null if the comparands are identical.
        /// </summary>
        public string Message { get; }

        public DiffResult(string name, bool areSame, string message)
        {
            Name = name;
            _entries = s_emptyEntries;
            AreSame = areSame;
            Message = areSame ? null : message;
        }

        public DiffResult(string name, IEnumerable<DiffResult> entries)
        {
            Name = name;
            _entries = entries.ToArray();
            AreSame = _entries.Length == 0 || _entries.All(e => e.AreSame);
        }
    }
}