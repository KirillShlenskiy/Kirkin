using System.Collections.Generic;
using System.Data;

namespace Kirkin.Diff.Data
{
    public static class DataTableDiff
    {
        public static IDiffResult Compare(DataTable x, DataTable y)
        {
            return Compare("DataTable", x, y);
        }

        internal static IDiffResult Compare(string name, DataTable x, DataTable y)
        {
            List<IDiffResult> entries = new List<IDiffResult>();

            IDiffResult columnCount = new SimpleDiffResult(
                "Column count", x.Columns.Count == y.Columns.Count, $"{x.Columns.Count} vs {y.Columns.Count}."
            );

            entries.Add(columnCount);

            IDiffResult rowCount = new SimpleDiffResult(
                "Row count", x.Rows.Count == y.Rows.Count, $"{x.Rows.Count} vs {y.Rows.Count}."
            );

            entries.Add(rowCount);

            if (columnCount.AreSame && rowCount.AreSame) {
                entries.Add(new MultiDiffResult("Rows", EnumerateRowDiff(x, y)));
            }

            return new MultiDiffResult(name, entries);
        }

        private static IEnumerable<IDiffResult> EnumerateRowDiff(DataTable x, DataTable y)
        {
            for (int i = 0; i < x.Rows.Count; i++) {
                yield return DataRowDiff.Compare($"Row {i}", x.Rows[i], y.Rows[i]);
            }
        }
    }
}