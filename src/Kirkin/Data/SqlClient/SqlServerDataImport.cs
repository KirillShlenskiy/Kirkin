#if !__MOBILE__

using System;
using System.Data;
using System.Data.SqlClient;

namespace Kirkin.Data.SqlClient
{
    /// <summary>
    /// SQL Server data import utility type.
    /// </summary>
    public class SqlServerDataImport
    {
        /// <summary>
        /// Connection string specified when this instance was created.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Creates a new <see cref="SqlServerDataImport"/> instance with the given connection string.
        /// </summary>
        public SqlServerDataImport(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException("Connection string cannot be null or empty.");

            ConnectionString = connectionString;
        }

        /// <summary>
        /// Imports the given <see cref="DataTable"/> to SQL Server (both schema and data).
        /// Drops and re-creates the target table in the process.
        /// </summary>
        /// <param name="dataTable">DataTable to import.</param>
        /// <param name="tableName">Target SQL server table. Will be dropped and re-created.</param>
        public void ImportDataTable(DataTable dataTable, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                SqlServerTableBuilder tableBuilder = ResolveTableBuilder(connection);

                tableBuilder.DropAndReCreateSqlTable(tableName, dataTable);

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = tableName;

                    bulkCopy.WriteToServer(dataTable);
                }
            }
        }

        /// <summary>
        /// Produces the <see cref="SqlServerTableBuilder"/> instance to be used for the import.
        /// </summary>
        protected virtual SqlServerTableBuilder ResolveTableBuilder(SqlConnection connection)
        {
            return new SqlServerTableBuilder(connection);
        }
    }
}

#endif