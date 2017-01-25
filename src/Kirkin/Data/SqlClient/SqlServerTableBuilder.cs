using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Kirkin.Data.SqlClient
{
    internal class SqlServerTableBuilder
    {
        // Reference: https://msdn.microsoft.com/en-us/library/bb386947(v=vs.110).aspx
        private static readonly Dictionary<Type, string> ClrSqlTypeMappings = new Dictionary<Type, string> {
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
        /// Connection specified when this instance was created.
        /// </summary>
        public SqlConnection Connection { get; }

        public SqlServerTableBuilder(SqlConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            Connection = connection;
        }

        /// <summary>
        /// Creates an SQL Server table with schema compatible with the given <see cref="DataTable"/>.
        /// Drops the target table if it already exists.
        /// </summary>
        public void CreateSqlTable(string tableName, DataTable dataTable)
        {
            string sql = GetCreateTableSql(tableName, dataTable);

            sql = $"IF OBJECT_ID('{tableName}') IS NOT NULL DROP TABLE [{tableName}];" + Environment.NewLine + Environment.NewLine + sql;

            if (Connection.State != ConnectionState.Open) {
                Connection.Open();
            }

            using (SqlCommand command = new SqlCommand(sql, Connection))
            {
                command.CommandType = CommandType.Text;

                command.ExecuteNonQuery();
            }
        }

        private string GetCreateTableSql(string tableName, DataTable dataTable)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine($"CREATE TABLE [{tableName}] (");

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                DataColumn column = dataTable.Columns[i];

                sql.Append($"  [{column.ColumnName}] {SqlTypeForColumn(column)}");

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

        protected virtual string SqlTypeForColumn(DataColumn column)
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

            if (ClrSqlTypeMappings.TryGetValue(column.DataType, out sqlType)) {
                return sqlType + " " + nullOrNotNull;
            }

            throw new NotSupportedException($"Unsupported data type for column '{column.ColumnName}'.");
        }
    }
}