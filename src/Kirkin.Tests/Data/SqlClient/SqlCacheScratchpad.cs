using System;
using System.Data;
using System.Data.SqlClient;

using Kirkin.Data.SqlClient;

using Newtonsoft.Json;

using NUnit.Framework;

namespace Kirkin.Tests.Data.SqlClient
{
    public class SqlCacheScratchpad
    {
        [Test]
        [Ignore("Early work.")]
        public void BasicSelect()
        {
            SqlCache cache = new SqlCache();

            for (int i = 0; i < 50; i++)
            {
                using (SqlConnection connection = new SqlConnection("Data Source=.; Initial Catalog=<>; Integrated Security=True;"))
                {
                    connection.Open();

                    //using (SqlCommand command = new SqlCommand("UPDATE Person SET DisplayName = DisplayName WHERE PersonID = 1", connection))
                    //{
                    //    command.ExecuteNonQuery();
                    //}

                    using (SqlCommand command = new SqlCommand("SELECT * FROM Person", connection))
                    {
                        cache.ExecuteDataTable(command);
                        //command.ExecuteDataTable();
                    }
                }
            }
        }

        public class SqlCache
        {
            private string LastCommandInfo;
            private string LastLSN;
            private object LastResult; // DataTable or DataSet.

            public DataTable ExecuteDataTable(SqlCommand command)
            {
                ValidateCommand(command);

                string lsn = GetCurrentLsn(command.Connection);
                string commandInfo = JsonConvert.SerializeObject(command);

                if (string.Equals(lsn, LastLSN) && string.Equals(commandInfo, LastCommandInfo) && LastResult is DataTable) {
                    return (DataTable)LastResult;
                }

                DataTable result = command.ExecuteDataTable();

                LastLSN = lsn;
                LastCommandInfo = commandInfo;
                LastResult = result;

                return result;
            }

            private void ValidateCommand(SqlCommand command)
            {
                if (command.Connection == null) {
                    throw new ArgumentException("SqlCommand's Connection property cannot be null.");
                }
            }

            private static string GetCurrentLsn(SqlConnection connection)
            {
                // The connection must already be open.
                using (SqlCommand command = new SqlCommand("SELECT MAX([Current LSN]) from ::fn_dblog(default, default)", connection))
                {
                    object result = command.ExecuteScalar();

                    return result is DBNull
                        ? null
                        : (string)result;
                }
            }
        }
    }
}