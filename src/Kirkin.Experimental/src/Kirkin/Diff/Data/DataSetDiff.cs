using System.Collections.Generic;
using System.Data;

namespace Kirkin.Diff.Data
{
    public class DataSetDiff : IDiffEngine<DataSet>
    {
        public IDiffResult Compare(DataSet x, DataSet y)
        {
            if (x.Tables.Count != y.Tables.Count) {
                return new SimpleDiffResult(false, $"Table count mismatch: {x.Tables.Count} vs {y.Tables.Count}.");
            }

            DataTableDiff engine = new DataTableDiff();

            return new MultiDiffResult(() => EnumerateTableDiffs(engine, x, y));
        }

        private IEnumerable<IDiffResult> EnumerateTableDiffs(DataTableDiff engine, DataSet x, DataSet y)
        {
            for (int i = 0; i < x.Tables.Count; i++) {
                yield return engine.Compare(x.Tables[i], y.Tables[i]);
            }
        }
    }
}