#if !__MOBILE__

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Kirkin.Data.SqlClient
{
    /// <summary>
    /// Auto-creates SQL Server tables that model CLR objects (i.e. <see cref="DataTable"/>).
    /// </summary>
    public class SqlServerTableBuilder
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
        /// Produces the SQL statement used to create the table with the given
        /// name and schema compatible with the given <see cref="DataTable"/>.
        /// </summary>
        public virtual string GetCreateTableSql(string tableName, DataTable dataTable)
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

        /// <summary>
        /// Resolves the SQL Server type appropriate for the given <see cref="DataColumn"/> (i.e. int, varchar(255) etc).
        /// </summary>
        protected virtual string SqlTypeForColumn(DataColumn column)
        {
            string nullOrNotNull = column.AllowDBNull ? "NULL" : "NOT NULL";

            if (column.DataType == typeof(string)) {
                return $"nvarchar({ComputeVarcharColumnLength(column)}) COLLATE database_default {nullOrNotNull}";
            }

            if (ClrSqlTypeMappings.TryGetValue(column.DataType, out string sqlType)) {
                return sqlType + " " + nullOrNotNull;
            }

            throw new NotSupportedException($"Unsupported data type for column '{column.ColumnName}'.");
        }

        private static string ComputeVarcharColumnLength(DataColumn column)
        {
            if (column.MaxLength == -1) return "4000"; // Same as Entity Framework.
            if (column.MaxLength > 8000) return "MAX";

            return column.MaxLength.ToString();

            //int maxLength = 0;

            //foreach (DataRow row in column.Table.Rows)
            //{
            //    string value = row[column] as string;

            //    if (value != null && value.Length > maxLength) {
            //        maxLength = value.Length;
            //    }
            //}

            //if (maxLength > 4000) return "MAX";
            //if (maxLength > 2000) return "4000";
            //if (maxLength > 1000) return "2000";
            //if (maxLength > 255) return "1000";

            //return "255";
        }
    }
}

#endif