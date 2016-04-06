using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Kirkin.Diff
{
    public static class DataTableDiff
    {
        public static DataTableDiffResult Compare(DataTable x, DataTable y)
        {
            if (x.Columns.Count != y.Columns.Count) {
                return new DataTableDiffResult(false, $"Column count mismatch: {x.Columns.Count} vs {y.Columns.Count}.", x, y);
            }

            if (x.Rows.Count != y.Rows.Count) {
                return new DataTableDiffResult(false, $"Row count mismatch: {x.Rows.Count} vs {y.Rows.Count}.", x, y);
            }

            if (new DataTableContentEqualityComparer().Equals(x, y)) {
                return new DataTableDiffResult(false, "Content mismatch.", x, y);
            }

            return new DataTableDiffResult(true, null, x, y);
        }

        sealed class DataTableContentEqualityComparer : IEqualityComparer<DataTable>
        {
            public bool Equals(DataTable x, DataTable y)
            {

                for (int i = 0; i < x.Rows.Count; i++)
                {
                    if (!DataCellEqualityComparer.Instance.Equals(x.Rows[i].ItemArray, y.Rows[i].ItemArray)) {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(DataTable obj)
            {
                throw new NotSupportedException();
            }

            sealed class DataCellEqualityComparer
                : IEqualityComparer<object>, IEqualityComparer
            {
                public static readonly DataCellEqualityComparer Instance = new DataCellEqualityComparer();

                private DataCellEqualityComparer()
                {
                }

                public new bool Equals(object x, object y)
                {
                    IStructuralEquatable strEqX = x as IStructuralEquatable;

                    if (strEqX != null)
                    {
                        IStructuralEquatable strEqY = y as IStructuralEquatable;

                        if (strEqY != null) {
                            // Most likely array comparison.
                            return strEqX.Equals(strEqY, Instance);
                        }
                    }

                    return object.Equals(x, y);
                }

                public int GetHashCode(object obj)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}