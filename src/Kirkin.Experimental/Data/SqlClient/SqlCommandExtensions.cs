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
        /// Executes the data reader on the given command and
        /// streams the result as a sequence of dictionary objects.
        /// </summary>
        public static IEnumerable<Dictionary<string, object>> ExecuteDictionary(this SqlCommand command)
        {
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