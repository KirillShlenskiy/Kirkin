using System.Collections.Generic;
using System.Data;

namespace Kirkin.Diff.Data
{
    public class DataSetDiff : CompositeDiffResult<DataSet>
    {
        internal DataSetDiff(DataSet x, DataSet y)
            : base(x, y)
        {
        }

        protected override IEnumerable<IDiffResult> Compare(DataSet x, DataSet y)
        {
            if (x.Tables.Count != y.Tables.Count)
            {
                yield return new SimpleDiffResult(false, $"Table count mismatch: {x.Tables.Count} vs {y.Tables.Count}.");
            }
            else
            {
                for (int i = 0; i < x.Tables.Count; i++) {
                    yield return new DataTableDiff(x.Tables[i], y.Tables[i]);
                }
            }
        }
    }
}