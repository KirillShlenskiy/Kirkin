using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Kirkin.Diff.Data
{
    public static class DataSetDiff
    {
        public static DiffResult Compare(DataSet x, DataSet y)
        {
            return Compare("DataSet", x, y);
        }

        internal static DiffResult Compare(string name, DataSet x, DataSet y)
        {
            List<DiffResult> entries = new List<DiffResult>();

            DiffResult tableCount = new DiffResult(
                "Table count", x.Tables.Count == y.Tables.Count, $"{x.Tables.Count} vs {y.Tables.Count}."
            );

            entries.Add(tableCount);

            if (tableCount.AreSame) {
                entries.Add(new DiffResult("Tables", EnumerateTableDiffs(x, y).ToArray()));
            }

            return new DiffResult(name, entries.ToArray());
        }

        private static IEnumerable<DiffResult> EnumerateTableDiffs(DataSet x, DataSet y)
        {
            for (int i = 0; i < x.Tables.Count; i++) {
                yield return DataTableDiff.Compare($"Table {i}", x.Tables[i], y.Tables[i]);
            }
        }
    }
}