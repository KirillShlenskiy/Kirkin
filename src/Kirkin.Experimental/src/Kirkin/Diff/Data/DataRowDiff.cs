using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Kirkin.Diff.Data
{
    internal static class DataRowDiff
    {
        internal static DiffResult Compare(string name, DataRow x, DataRow y)
        {
            return new DiffResult("Cells", GetCellDiffs(x, y));
        }

        private static DiffResult[] GetCellDiffs(DataRow x, DataRow y)
        {
            List<DiffResult> entries = null;

            // Perf: prevent new array allocation on every ItemArray getter call.
            object[] xItemArray = x.ItemArray;
            object[] yItemArray = y.ItemArray;

            for (int i = 0; i < xItemArray.Length; i++)
            {
                if (!DataCellEqualityComparer.Instance.Equals(xItemArray[i], yItemArray[i]))
                {
                    if (entries == null) entries = new List<DiffResult>();

                    entries.Add(new DiffResult(x.Table.Columns[i].ColumnName, false, $"{xItemArray[i]} vs {yItemArray[i]}."));
                }
            }

            return entries == null
                ? DiffResult.EmptyDiffResultArray
                : entries.ToArray();
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