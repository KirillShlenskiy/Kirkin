using System;
using System.Linq;
using System.Text;

namespace Kirkin.Diff
{
    /// <summary>
    /// Represents the result of a diff operation.
    /// </summary>
    public sealed class DiffResult
    {
        /// <summary>
        /// Simle <see cref="DiffResult"/> factory.
        /// </summary>
        public static DiffResult Create(string name, object x, object y)
        {
            return PrimitiveEqualityComparer.Instance.Equals(x, y)
                ? new DiffResult(name, true, string.Empty)
                : new DiffResult(name, false, $"{ToString(x)} | {ToString(y)}");
        }

        private static string ToString(object obj)
        {
            if (obj == null) return "NULL";

            if (obj is Array)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append('[');

                foreach (object item in (Array)obj)
                {
                    if (sb.Length > 1) {
                        sb.Append(", ");
                    }

                    sb.Append(ToString(item));
                }

                sb.Append(']');
            }

            return obj.ToString();
        }

        internal static readonly DiffResult[] EmptyDiffResultArray = new DiffResult[0];

        /// <summary>
        /// Child diff entries.
        /// </summary>
        public DiffResult[] Entries { get; }

        public bool AreSame { get; }

        /// <summary>
        /// Name of the comparison.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Diff message or null if the comparands are identical.
        /// </summary>
        internal readonly string Message;

        public DiffResult(string name, bool areSame, string message)
        {
            Name = name;
            Entries = EmptyDiffResultArray;
            AreSame = areSame;
            Message = areSame ? null : message;
        }

        public DiffResult(string name, DiffResult[] entries)
        {
            Name = name;
            Entries = entries;
            AreSame = Entries.Length == 0 || Entries.All(e => e.AreSame);
        }

        public override string ToString()
        {
            return ToString(DiffTextFormat.Flat);
        }

        public string ToString(DiffTextFormat format)
        {
            if (format == DiffTextFormat.Flat) return DiffDescriptionBuilder.BuildFlatDiffMessage(this);
            if (format == DiffTextFormat.Indented) return DiffDescriptionBuilder.BuildIndentedDiffMessage(this);

            throw new NotImplementedException($"Unknown {nameof(DiffTextFormat)} value.");
        }

        static class DiffDescriptionBuilder
        {
            public static string BuildFlatDiffMessage(DiffResult diffResult)
            {
                if (diffResult == null) throw new ArgumentNullException(nameof(diffResult));

                StringBuilder sb = new StringBuilder();

                BuildFlatMessage(sb, string.Empty, diffResult);

                return sb.ToString();
            }

            private static void BuildFlatMessage(StringBuilder sb, string line, DiffResult diffResult)
            {
                if (!diffResult.AreSame)
                {
                    if (line.Length != 0) {
                        line += " -> ";
                    }

                    line += diffResult.Name;

                    if (!string.IsNullOrEmpty(diffResult.Message))
                    {
                        line += ": ";
                        line += diffResult.Message;
                    }

                    if (diffResult.Entries.Length == 0)
                    {
                        if (sb.Length != 0) {
                            sb.AppendLine();
                        }

                        sb.Append(line);
                    }
                    else
                    {
                        foreach (DiffResult childEntry in diffResult.Entries) {
                            BuildFlatMessage(sb, line, childEntry);
                        }
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
                    sb.Append(diffResult.Message);
                    sb.AppendLine();

                    foreach (DiffResult childEntry in diffResult.Entries) {
                        BuildIndentedMessage(sb, indenting + 1, childEntry);
                    }
                }
            }
        }
    }
}