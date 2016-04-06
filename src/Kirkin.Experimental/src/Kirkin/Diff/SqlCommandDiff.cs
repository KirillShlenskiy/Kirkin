using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Kirkin.src.Kirkin.Diff
{
    public static class SqlCommandDiff
    {
        public static bool CompareResultSets(SqlCommand cmd1, SqlCommand cmd2)
        {
            using (DataSet ds1 = ExecuteDataSet(cmd1))
            using (DataSet ds2 = ExecuteDataSet(cmd2))
            {
                if (ds1.Tables.Count != ds2.Tables.Count) return false;

                DataTableContentEqualityComparer dtComparer = new DataTableContentEqualityComparer();

                for (int i = 0; i < ds1.Tables.Count; i++)
                {
                    if (!dtComparer.Equals(ds1.Tables[i], ds2.Tables[i])) {
                        return false;
                    }
                }

                return true;
            }
        }

        private static DataSet ExecuteDataSet(SqlCommand cmd)
        {
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();

                adapter.Fill(ds);

                return ds;
            }
        }

        sealed class DataTableContentEqualityComparer : IEqualityComparer<DataTable>
        {
            public bool Equals(DataTable x, DataTable y)
            {
                if (x.Columns.Count != y.Columns.Count) return false;
                if (x.Rows.Count != y.Rows.Count) return false;

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
                throw new NotImplementedException();
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
