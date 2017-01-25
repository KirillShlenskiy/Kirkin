using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Kirkin.Data.SqlClient
{
    public sealed class SqlServerDataImport
    {
        public string ConnectionString { get; }

        public SqlServerDataImport(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public void CreateTableFromDataTable(string tableName, DataTable dataTable)
        {
            string sql = GetCreateTableSql(tableName, dataTable);

            sql = $"IF OBJECT_ID('{tableName}') IS NOT NULL DROP TABLE [{tableName}];" + Environment.NewLine + Environment.NewLine + sql;

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;

                    command.ExecuteNonQuery();
                }
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
                {
                    length = "MAX";
                }
                else if (maxLength > 2000)
                {
                    length = "4000";
                }
                else if (maxLength > 1000)
                {
                    length = "2000";
                }
                else if (maxLength > 255)
                {
                    length = "1000";
                }

                return $"varchar({length}) {nullOrNotNull}";
            }

            if (column.DataType == typeof(int)) return "int " + nullOrNotNull;
            if (column.DataType == typeof(decimal)) return "money " + nullOrNotNull;

            throw new NotSupportedException($"Unsupported data type for column '{column.ColumnName}'.");
        }
    }
}