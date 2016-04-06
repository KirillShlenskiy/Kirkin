using System.Collections.Generic;
using System.Data;

namespace Kirkin.Diff.Data
{
    public static class DataTableDiff
    {
        public static DiffResult Compare(DataTable x, DataTable y)
        {
            return Compare("DataTable", x, y);
        }

        internal static DiffResult Compare(string name, DataTable x, DataTable y)
        {
            List<DiffResult> entries = new List<DiffResult>();

            DiffResult columnCount = new DiffResult(
                "Column count", x.Columns.Count == y.Columns.Count, $"{x.Columns.Count} vs {y.Columns.Count}."
            );

            entries.Add(columnCount);

            if (columnCount.AreSame)
            {
                entries.Add(new DiffResult("Column names", EnumerateColumnNameDiffs(x, y)));
                entries.Add(new DiffResult("Column types", EnumerateColumnDataTypeDiffs(x, y)));
            }

            DiffResult rowCount = new DiffResult(
                "Row count", x.Rows.Count == y.Rows.Count, $"{x.Rows.Count} vs {y.Rows.Count}."
            );

            entries.Add(rowCount);

            if (columnCount.AreSame && rowCount.AreSame) {
                entries.Add(new DiffResult("Rows", EnumerateRowDiffs(x, y)));
            }

            return new DiffResult(name, entries);
        }

        private static IEnumerable<DiffResult> EnumerateColumnNameDiffs(DataTable x, DataTable y)
        {
            for (int i = 0; i < x.Columns.Count; i++)
            {
                yield return new DiffResult(
                    $"Column {i}", x.Columns[i].ColumnName == y.Columns[i].ColumnName, $"{x.Columns[i].ColumnName} vs {y.Columns[i].ColumnName}"
                );
            }
        }

        private static IEnumerable<DiffResult> EnumerateColumnDataTypeDiffs(DataTable x, DataTable y)
        {
            for (int i = 0; i < x.Columns.Count; i++)
            {
                yield return new DiffResult(
                    $"Column {i}", x.Columns[i].DataType == y.Columns[i].DataType, $"{x.Columns[i].DataType.Name} vs {y.Columns[i].DataType.Name}"
                );
            }
        }

        private static IEnumerable<DiffResult> EnumerateRowDiffs(DataTable x, DataTable y)
        {
            for (int i = 0; i < x.Rows.Count; i++) {
                yield return DataRowDiff.Compare($"Row {i}", x.Rows[i], y.Rows[i]);
            }
        }
    }
}