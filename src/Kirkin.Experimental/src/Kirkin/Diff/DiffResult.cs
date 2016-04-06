using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kirkin.Diff
{
    /// <summary>
    /// Represents the result of a diff operation.
    /// </summary>
    public sealed class DiffResult
    {
        private static readonly DiffResult[] s_emptyEntries = new DiffResult[0];
        private readonly DiffResult[] _entries;
        internal readonly string _message;

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
        public string Message
        {
            get
            {
                return DiffDescriptionBuilder.BuildIndentedDiffMessage(this);
            }
        }

        public DiffResult(string name, bool areSame, string message)
        {
            Name = name;
            _entries = s_emptyEntries;
            AreSame = areSame;
            _message = areSame ? null : message;
        }

        public DiffResult(string name, IEnumerable<DiffResult> entries)
        {
            Name = name;
            _entries = entries.ToArray();
            AreSame = _entries.Length == 0 || _entries.All(e => e.AreSame);
        }

        public override string ToString()
        {
            return DiffDescriptionBuilder.BuildFlatDiffMessage(this);
        }

        static class DiffDescriptionBuilder
        {
            public static string BuildFlatDiffMessage(DiffResult diffResult)
            {
                if (diffResult == null) throw new ArgumentNullException(nameof(diffResult));

                StringBuilder sb = new StringBuilder();

                BuildFlatMessage(sb, diffResult);

                return sb.ToString();
            }

            private static void BuildFlatMessage(StringBuilder sb, DiffResult diffResult)
            {
                if (!diffResult.AreSame)
                {
                    if (sb.Length != 0) {
                        sb.Append(" -> ");
                    }

                    sb.Append(diffResult.Name);

                    if (!string.IsNullOrEmpty(diffResult._message))
                    {
                        sb.Append(": ");
                        sb.Append(diffResult._message);
                    }

                    foreach (DiffResult childEntry in diffResult.Entries) {
                        BuildFlatMessage(sb, childEntry);
                    }
                }
            }

            public static string BuildIndentedDiffMessage(DiffResult diffResult)
            {
                if (diffResult == null) throw new ArgumentNullException(nameof(diffResult));

                StringBuilder sb = new StringBuilder();

                BuildIndentedMessage(sb, 0, diffResult);

                return sb.ToString();
            }

            private static void BuildIndentedMessage(StringBuilder sb, int indenting, DiffResult diffResult)
            {
                if (!diffResult.AreSame)
                {
                    if (indenting != 0) {
                        sb.Append(new string(' ', indenting * 3));
                    }

                    sb.Append(diffResult.Name);
                    sb.Append(": ");
                    sb.Append(diffResult._message);
                    sb.AppendLine();

                    foreach (DiffResult childEntry in diffResult.Entries) {
                        BuildIndentedMessage(sb, indenting + 1, childEntry);
                    }
                }
            }
        }
    }
}