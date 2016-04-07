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
            List<DiffResult> results = new List<DiffResult>();

            DiffResult cellCount = new DiffResult(
                "ItemArray length", x.ItemArray.Length == y.ItemArray.Length, $"{x.ItemArray.Length} vs {y.ItemArray.Length}."
            );

            results.Add(cellCount);

            if (cellCount.AreSame) {
                results.Add(new DiffResult("Cells", GetCellDiffs(x, y)));
            }

            return new DiffResult(name, results.ToArray());
        }

        private static DiffResult[] GetCellDiffs(DataRow x, DataRow y)
        {
            List<DiffResult> entries = null;

            for (int i = 0; i < x.ItemArray.Length; i++)
            {
                if (!DataCellEqualityComparer.Instance.Equals(x.ItemArray[i], y.ItemArray[i]))
                {
                    if (entries == null) entries = new List<DiffResult>();

                    entries.Add(new DiffResult(x.Table.Columns[i].ColumnName, false, $"{x.ItemArray[i]} vs {y.ItemArray[i]}."));
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