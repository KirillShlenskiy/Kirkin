#if !__MOBILE__

using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Kirkin.Data.SqlClient
{
    /// <summary>
    /// SQL Server data import utility type.
    /// </summary>
    public class SqlServerDataImport
    {
        private static readonly SqlServerTableBuilder DefaultSqlServerTableBuilder = new SqlServerTableBuilder();
        
        // SqlConnection or connection string.
        private readonly object ConnectionObj;

        /// <summary>
        /// Connection string specified when this instance was created.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                if (ConnectionObj is SqlConnection connection) {
                    return connection.ConnectionString;
                }

                return (string)ConnectionObj;
            }
        }

        /// <summary>
        /// Creates a new <see cref="SqlServerDataImport"/> instance with the given connection string.
        /// </summary>
        public SqlServerDataImport(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException("Connection string cannot be null or empty.");

            ConnectionObj = connectionString;
        }

        /// <summary>
        /// Creates a new <see cref="SqlServerDataImport"/> instance with the given <see cref="SqlConnection"/>.
        /// </summary>
        public SqlServerDataImport(SqlConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            ConnectionObj = connection;
        }

        /// <summary>
        /// Imports the given <see cref="DataTable"/> to SQL Server (both schema and data).
        /// </summary>
        /// <param name="dataTable">DataTable to import.</param>
        /// <param name="tableName">Target SQL server table. Will be dropped and re-created.</param>
        /// <param name="dropAndReCreateTable">Determines whether the table will be dropped and re-created if it already exists.</param>
        public void ImportDataTable(DataTable dataTable, string tableName, bool dropAndReCreateTable = false)
        {
            SqlServerTableBuilder tableBuilder = ResolveTableBuilder();
            string createTableSql = tableBuilder.GetCreateTableSql(tableName, dataTable);
            StringBuilder sql = new StringBuilder();

            if (dropAndReCreateTable)
            {
                sql.AppendLine($"IF OBJECT_ID('{tableName}') IS NOT NULL DROP TABLE [{tableName}];");
            }
            else
            {
                sql.AppendLine($"IF OBJECT_ID('{tableName}') IS NOT NULL RETURN;");
            }

            sql.AppendLine();
            sql.Append(createTableSql);

            SqlConnection connection = GetSqlConnection(out bool needToDisposeConnection);

            try
            {
                if (connection.State != ConnectionState.Open) {
                    connection.Open();
                }

                using (SqlCommand command = new SqlCommand(sql.ToString(), connection)) {
                    command.ExecuteNonQuery();
                }

                if (dataTable.Rows.Count != 0)
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.DestinationTableName = tableName;

                        bulkCopy.WriteToServer(dataTable);
                    }
                }
            }
            finally
            {
                if (needToDisposeConnection) {
                    connection.Dispose();
                }
            }
        }

        /// <summary>
        /// Resolves the <see cref="SqlConnection"/> instance to be used by this import.
        /// </summary>
        protected virtual SqlConnection GetSqlConnection(out bool needToDispose)
        {
            if (ConnectionObj is SqlConnection connection)
            {
                needToDispose = false;

                return connection;
            }

            needToDispose = true;

            return new SqlConnection((string)ConnectionObj);
        }

        /// <summary>
        /// Produces the <see cref="SqlServerTableBuilder"/> instance to be used for the import.
        /// </summary>
        protected virtual SqlServerTableBuilder ResolveTableBuilder()
        {
            return DefaultSqlServerTableBuilder;
        }
    }
}

#endif