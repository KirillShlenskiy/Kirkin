#if !__MOBILE__

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Kirkin.Data.SqlClient
{
    /// <summary>
    /// Common <see cref="SqlCommand"/> extension methods.
    /// </summary>
    public static class SqlCommandExtensions
    {
        /// <summary>
        /// Creates a <see cref="DataTable"/> and populates it using the result set of the given command.
        /// </summary>
        public static DataSet ExecuteDataSet(this SqlCommand command)
        {
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                DataSet dataSet = new DataSet();

                adapter.Fill(dataSet);

                return dataSet;
            }
        }

        /// <summary>
        /// Creates a <see cref="DataTable"/> and populates it using the result set of the given command.
        /// </summary>
        public static DataTable ExecuteDataTable(this SqlCommand command)
        {
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                DataTable dataTable = new DataTable();

                adapter.Fill(dataTable);

                return dataTable;
            }
        }

        /// <summary>
        /// Executes the data reader on the given command and
        /// streams the result as a sequence of dictionary objects.
        /// Opens the connection if necessary.
        /// </summary>
        public static IEnumerable<Dictionary<string, object>> ExecuteDictionary(this SqlCommand command)
        {
            if (command.Connection == null) throw new InvalidOperationException("The connection property has not been initialized.");

            if (command.Connection.State != ConnectionState.Open) {
                command.Connection.Open();
            }

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read()) {
                    yield return ReaderToDictionary(reader);
                }
            }
        }

        /// <summary>
        /// Executes the data reader on the given command and
        /// streams the result as a sequence of dictionary objects.
        /// </summary>
        public static IEnumerable<Dictionary<string, object>> ExecuteDictionary(this SqlCommand command, CommandBehavior behavior)
        {
            if (command.Connection == null) throw new InvalidOperationException("The connection property has not been initialized.");

            if (command.Connection.State != ConnectionState.Open) {
                command.Connection.Open();
            }

            using (SqlDataReader reader = command.ExecuteReader(behavior))
            {
                while (reader.Read()) {
                    yield return ReaderToDictionary(reader);
                }
            }
        }

        internal static Dictionary<string, object> ReaderToDictionary(SqlDataReader reader)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string name = reader.GetName(i);
                object value = reader[i];

                if (value == DBNull.Value) {
                    value = null;
                }

                dict.Add(name, value);
            }

            return dict;
        }
    }
}

#endif