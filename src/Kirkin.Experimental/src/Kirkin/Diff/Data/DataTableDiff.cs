using System.Collections.Generic;
using System.Data;

namespace Kirkin.Diff.Data
{
    public sealed class DataTableDiff : IDiffEngine<DataTable>
    {
        public IDiffResult Compare(DataTable x, DataTable y)
        {
            if (x.Columns.Count != y.Columns.Count) {
                return new SimpleDiffResult(false, $"Column count mismatch: {x.Columns.Count} vs {y.Columns.Count}.");
            }

            if (x.Rows.Count != y.Rows.Count) {
                return new SimpleDiffResult(false, $"Row count mismatch: {x.Rows.Count} vs {y.Rows.Count}.");
            }

            DataRowDiff engine = new DataRowDiff();

            return new MultiDiffResult(() => EnumerateRowDiff(engine, x, y));
        }

        private IEnumerable<IDiffResult> EnumerateRowDiff(DataRowDiff engine, DataTable x, DataTable y)
        {
            for (int i = 0; i < x.Rows.Count; i++) {
                yield return engine.Compare(x.Rows[i], y.Rows[i]);
            }
        }
    }
}