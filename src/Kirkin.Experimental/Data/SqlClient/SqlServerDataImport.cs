using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

            HashSet<DataColumn> keyColumns = new HashSet<DataColumn>(dataTable.PrimaryKey);

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                DataColumn column = dataTable.Columns[i];

                sql.Append($"  [{column.ColumnName}] {SqlTypeFromColumn(column)}");

                if (keyColumns.Contains(column)) {
                    sql.Append(" PRIMARY KEY");
                }

                if (i == dataTable.Columns.Count - 1)
                {
                    sql.AppendLine();
                }
                else
                {
                    sql.AppendLine(",");
                }
            }

            sql.AppendLine(");");

            return sql.ToString();
        }

        private static string SqlTypeFromColumn(DataColumn column)
        {
            if (column.DataType == typeof(string))
            {
                // TODO: Better length resolution.
                return "varchar(255) NULL";
            }

            string nullOrNotNull = column.AllowDBNull ? "NULL" : "NOT NULL";

            if (column.DataType == typeof(int)) return $"int {nullOrNotNull}";
            if (column.DataType == typeof(decimal)) return $"money {nullOrNotNull}";

            throw new NotSupportedException($"Unsupported data type for column '{column.ColumnName}'.");
        }
    }
}