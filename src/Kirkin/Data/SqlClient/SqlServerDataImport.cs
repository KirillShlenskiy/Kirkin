using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Kirkin.Data.SqlClient
{
    /// <summary>
    /// SQL Server data import utility type.
    /// </summary>
    public sealed class SqlServerDataImport
    {
        // Reference: https://msdn.microsoft.com/en-us/library/bb386947(v=vs.110).aspx
        private static readonly Dictionary<Type, string> ClrSqlTypeMapping = new Dictionary<Type, string> {
            { typeof(bool), "bit" },
            { typeof(byte), "tinyint" },
            { typeof(short), "smallint" },
            { typeof(int), "int" },
            { typeof(long), "bigint" },
            { typeof(sbyte), "smallint" },
            { typeof(ushort), "int" },
            { typeof(uint), "bigint" },
            { typeof(ulong), "decimal(20)" },
            { typeof(decimal), "decimal(29, 4)" },
            { typeof(float), "real" },
            { typeof(double), "float" },
            { typeof(char), "nchar(1)" },
            { typeof(DateTime), "datetime" },
            { typeof(DateTimeOffset), "datetimeoffset" },
            { typeof(TimeSpan), "time" },
            { typeof(byte[]), "varbinary(MAX)" },
            { typeof(Guid), "uniqueidentifier" },
            { typeof(object), "sql_variant" }
        };

        /// <summary>
        /// Connection string specified when this instance was created.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Creates a new <see cref="SqlServerDataImport"/> instance with the given connection string.
        /// </summary>
        public SqlServerDataImport(string connectionString)
        {
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
                CreateSqlTableFromDataTableSchema(tableName, dataTable);

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = tableName;

                    bulkCopy.WriteToServer(dataTable);
                }
            }
        }

        /// <summary>
        /// Creates an SQL Server table with schema compatible with the given <see cref="DataTable"/>.
        /// Drops the target table if it already exists.
        /// </summary>
        public void CreateSqlTableFromDataTableSchema(string tableName, DataTable dataTable)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                CreateSqlTableFromDataTableSchema(tableName, dataTable, connection);
            }
        }

        private void CreateSqlTableFromDataTableSchema(string tableName, DataTable dataTable, SqlConnection connection)
        {
            string sql = GetCreateTableSql(tableName, dataTable);

            sql = $"IF OBJECT_ID('{tableName}') IS NOT NULL DROP TABLE [{tableName}];" + Environment.NewLine + Environment.NewLine + sql;

            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;

                command.ExecuteNonQuery();
            }
        }

        private static string GetCreateTableSql(string tableName, DataTable dataTable)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine($"CREATE TABLE [{tableName}] (");

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                DataColumn column = dataTable.Columns[i];

                sql.Append($"  [{column.ColumnName}] {SqlTypeFromColumn(column)}");

                if (i == dataTable.Columns.Count - 1 && dataTable.PrimaryKey.Length == 0)
                {
                    sql.AppendLine();
                }
                else
                {
                    sql.AppendLine(",");
                }
            }

            if (dataTable.PrimaryKey.Length != 0)
            {
                sql.Append("  PRIMARY KEY (");
                sql.Append(string.Join(", ", dataTable.PrimaryKey.Select(c => "[" + c.ColumnName + "]")));
                sql.AppendLine(")");
            }

            sql.AppendLine(");");

            return sql.ToString();
        }

        private static string SqlTypeFromColumn(DataColumn column)
        {
            string nullOrNotNull = column.AllowDBNull ? "NULL" : "NOT NULL";

            if (column.DataType == typeof(string))
            {
                int maxLength = 0;

                foreach (DataRow row in column.Table.Rows)
                {
                    string value = row[column] as string;

                    if (value != null && value.Length > maxLength) {
                        maxLength = value.Length;
                    }
                }

                string length = "255";

                if (maxLength > 4000)
                    length = "MAX";
                else if (maxLength > 2000)
                    length = "4000";
                else if (maxLength > 1000)
                    length = "2000";
                else if (maxLength > 255)
                    length = "1000";

                return $"nvarchar({length}) {nullOrNotNull}";
            }

            string sqlType;

            if (ClrSqlTypeMapping.TryGetValue(column.DataType, out sqlType)) {
                return sqlType + " " + nullOrNotNull;
            }

            throw new NotSupportedException($"Unsupported data type for column '{column.ColumnName}'.");
        }
    }
}