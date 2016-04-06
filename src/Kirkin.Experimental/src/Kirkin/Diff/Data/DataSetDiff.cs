using System.Collections.Generic;
using System.Data;

namespace Kirkin.Diff.Data
{
    public class DataSetDiff : IDiffEngine<DataSet>
    {
        public IDiffResult Compare(DataSet x, DataSet y)
        {
            return Compare("DataSet", x, y);
        }

        public IDiffResult Compare(string name, DataSet x, DataSet y)
        {
            List<IDiffResult> entries = new List<IDiffResult>();

            IDiffResult tableCount = new SimpleDiffResult(
                "Table count", x.Tables.Count == y.Tables.Count, $"{x.Tables.Count} vs {y.Tables.Count}."
            );

            entries.Add(tableCount);

            if (tableCount.AreSame)
            {
                DataTableDiff engine = new DataTableDiff();

                entries.Add(new MultiDiffResult("Tables", EnumerateTableDiffs(engine, x, y)));
            }

            return new MultiDiffResult(name, entries);
        }

        private IEnumerable<IDiffResult> EnumerateTableDiffs(DataTableDiff engine, DataSet x, DataSet y)
        {
            for (int i = 0; i < x.Tables.Count; i++) {
                yield return engine.Compare($"Table {i}", x.Tables[i], y.Tables[i]);
            }
        }
    }
}