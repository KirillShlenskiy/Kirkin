using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Kirkin.Diff.Data
{
    internal sealed class DataRowDiff : CompositeDiffResult<DataRow>
    {
        internal DataRowDiff(DataRow x, DataRow y)
            : base(x, y)
        {
        }

        protected override IEnumerable<IDiffResult> Compare(DataRow x, DataRow y)
        {
            if (x.ItemArray.Length != y.ItemArray.Length)
            {
                yield return new SimpleDiffResult(false, $"ItemArray length mismatch: {x.ItemArray.Length} vs {y.ItemArray.Length}.");
            }
            else
            {
                for (int i = 0; i < x.ItemArray.Length; i++)
                {
                    if (!DataCellEqualityComparer.Instance.Equals(x.ItemArray[i], y.ItemArray[i]))
                    {
                        yield return new SimpleDiffResult(false, $"{x.Table.Columns[i]} diff: {x.ItemArray[i]} vs {y.ItemArray[i]}.");
                    }
                }
            }
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