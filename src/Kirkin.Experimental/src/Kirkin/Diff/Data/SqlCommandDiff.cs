using System.Data;
using System.Data.SqlClient;

namespace Kirkin.Diff.Data
{
    public static class SqlCommandDiff
    {
        public static DiffResult CompareResultSets(SqlCommand cmd1, SqlCommand cmd2)
        {
            using (DataSet ds1 = ExecuteDataSet(cmd1))
            using (DataSet ds2 = ExecuteDataSet(cmd2))
            {
                return DataSetDiff.Compare(ds1, ds2);
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