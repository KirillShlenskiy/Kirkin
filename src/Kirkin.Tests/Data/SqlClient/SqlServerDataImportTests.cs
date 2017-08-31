using System.Data;
using System.Data.SqlClient;

using Kirkin.Data;
using Kirkin.Data.SqlClient;

using NUnit.Framework;

namespace Kirkin.Tests.Data.SqlClient
{
    public sealed class SqlServerDataImportTests
    {
        private static readonly string ConnectionString = new SqlConnectionStringBuilder {
            DataSource = ".",
            InitialCatalog = "Test",
            IntegratedSecurity = true
        }.ToString();

        //public SqlServerDataImportTests()
        //{
        //    if (!Environment.MachineName.Equals("KIRKINPUTER", StringComparison.OrdinalIgnoreCase)) {
        //        Assert.Ignore("This test only runs on KIRKINPUTER.");
        //    }
        //}

        [Test]
        public void BasicTestConnectionString()
        {
            using (DataTable dt = CreateTestDataTable())
            {
                SqlServerDataImport import = new SqlServerDataImport(ConnectionString);

                import.ImportDataTable(dt, "TestTable", dropAndReCreateTable: true);

                CheckDatabaseTableContents("TestTable", dt);
            }
        }

        [Test]
        public void BasicTestExistingSqlConnection()
        {
            using (DataTable dt = CreateTestDataTable())
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlServerDataImport import = new SqlServerDataImport(connection);

                    import.ImportDataTable(dt, "TestTable", dropAndReCreateTable: true);

                    CheckDatabaseTableContents("TestTable", dt);
                }
            }
        }

        private static DataTable CreateTestDataTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Value", typeof(decimal));

            dt.PrimaryKey = new[] {
                dt.Columns["ID"]
            };

            dt.Rows.Add(1, "Aaa", 123.45m);
            dt.Rows.Add(2, "Bbb", 321.00);

            return dt;
        }

        private static void CheckDatabaseTableContents(string tableName, DataTable referenceDt)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand($"SELECT * FROM [{tableName}]", connection))
                {
                    DataTableLite actualDt = command.ExecuteDataTableLite();

                    Assert.AreEqual(referenceDt.Rows.Count, actualDt.Rows.Count);

                    for (int row = 0; row < referenceDt.Rows.Count; row++)
                    for (int col = 0; col < referenceDt.Columns.Count; col++) {
                        Assert.AreEqual(referenceDt.Rows[row][col], actualDt.Rows[row][col]);
                    }
                }
            }
        }
    }
}