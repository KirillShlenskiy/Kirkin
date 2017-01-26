using System;
using System.Data;
using System.Data.SqlClient;

using Kirkin.Data.SqlClient;

using NUnit.Framework;

namespace Kirkin.Tests.Data.SqlClient
{
    public sealed class SqlServerDataImportTests
    {
        private static readonly string ConnectionString = new SqlConnectionStringBuilder {
            DataSource = @"KIRKINPUTER\SQL2008R2",
            InitialCatalog = "Test",
            IntegratedSecurity = true
        }.ToString();

        public SqlServerDataImportTests()
        {
            if (!Environment.MachineName.Equals("KIRKINPUTER", StringComparison.OrdinalIgnoreCase)) {
                Assert.Ignore("This test only runs on KIRKINPUTER.");
            }
        }

        [Test]
        public void BasicTest()
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

            SqlServerDataImport import = new SqlServerDataImport(ConnectionString);

            import.ImportDataTable(dt, "TestTable", dropAndReCreateTable: true);
        }
    }
}