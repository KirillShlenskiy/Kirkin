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
    }
}
