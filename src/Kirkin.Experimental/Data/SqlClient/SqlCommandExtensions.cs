using System.Data;
using System.Data.SqlClient;

namespace Kirkin.Data.SqlClient
{
    public static partial class SqlCommandExtensions2
    {
        public static DataSetLite ExecuteDataSetLite(this SqlCommand command)
        {
            using (SqlDataReader reader = command.ExecuteReader())
            {
                DataSetLite ds = new DataSetLite();

                while (true)
                {
                    ds.Tables.Add(TableFromReader(reader));

                    if (!reader.NextResult()) {
                        break;
                    }
                }

                return ds;
            }
        }

        public static DataTableLite ExecuteDataTableLite(this SqlCommand command)
        {
            if (command.Connection.State != ConnectionState.Open) {
                command.Connection.Open();
            }

            using (SqlDataReader reader = command.ExecuteReader()) {
                return TableFromReader(reader);
            }
        }

        private static DataTableLite TableFromReader(SqlDataReader reader)
        {
            DataTableLite table = new DataTableLite();

            for (int i = 0; i < reader.FieldCount; i++) {
                table.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            }

            while (reader.Read())
            {
                object[] itemArray = new object[reader.FieldCount];

                for (int i = 0; i < itemArray.Length; i++) {
                    itemArray[i] = reader[i];
                }

                table.Rows.Add(itemArray);
            }

            table.Rows.TrimExcess(); // Manage GC pressure.

            return table;
        }
    }
}