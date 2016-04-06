using System.Data;
using System.Data.SqlClient;

namespace Kirkin.Diff.Data
{
    public static class SqlCommandDiff
    {
        public static DataSetDiff CompareResultSets(SqlCommand cmd1, SqlCommand cmd2)
        {
            using (DataSet ds1 = ExecuteDataSet(cmd1))
            using (DataSet ds2 = ExecuteDataSet(cmd2))
            {
                DataSetDiff diff = new DataSetDiff(ds1, ds2);

                var ignored = diff.Entries;

                return diff;
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