using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Kirkin.Diff.Data
{
    public sealed class DataTableDiff : CompositeDiffResult<DataTable>
    {
        internal DataTableDiff(DataTable x, DataTable y)
            : base(x, y)
        {
        }

        protected override IEnumerable<IDiffResult> Compare(DataTable x, DataTable y)
        {
            if (x.Columns.Count != y.Columns.Count)
            {
                yield return new SimpleDiffResult(false, $"Column count mismatch: {x.Columns.Count} vs {y.Columns.Count}.");
            }
            else if (x.Rows.Count != y.Rows.Count)
            {
                yield return new SimpleDiffResult(false, $"Row count mismatch: {x.Rows.Count} vs {y.Rows.Count}.");
            }
            else
            {
                for (int i = 0; i < x.Rows.Count; i++) {
                    yield return new DataRowDiff(x.Rows[i], y.Rows[i]);
                }
            }
        }
    }
}