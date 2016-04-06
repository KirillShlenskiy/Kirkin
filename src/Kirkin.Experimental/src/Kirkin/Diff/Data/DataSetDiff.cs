using System.Collections.Generic;
using System.Data;

namespace Kirkin.Diff.Data
{
    public static class DataSetDiff
    {
        public static IDiffResult Compare(DataSet x, DataSet y)
        {
            return Compare("DataSet", x, y);
        }

        internal static IDiffResult Compare(string name, DataSet x, DataSet y)
        {
            List<IDiffResult> entries = new List<IDiffResult>();

            IDiffResult tableCount = new SimpleDiffResult(
                "Table count", x.Tables.Count == y.Tables.Count, $"{x.Tables.Count} vs {y.Tables.Count}."
            );

            entries.Add(tableCount);

            if (tableCount.AreSame) {
                entries.Add(new MultiDiffResult("Tables", EnumerateTableDiffs(x, y)));
            }

            return new MultiDiffResult(name, entries);
        }

        private static IEnumerable<IDiffResult> EnumerateTableDiffs(DataSet x, DataSet y)
        {
            for (int i = 0; i < x.Tables.Count; i++) {
                yield return DataTableDiff.Compare($"Table {i}", x.Tables[i], y.Tables[i]);
            }
        }
    }
}