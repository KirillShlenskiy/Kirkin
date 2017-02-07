using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Kirkin.Data.SqlClient
{
    public static class SqlDataReaderExtensions
    {
        // Key = field type.
        // Value = resolve value at index delegate (Func<SqlDataReader, int, T>).
        private static readonly Dictionary<Type, object> Delegates = new Dictionary<Type, object> {
            { typeof(bool), new Func<SqlDataReader, int, bool>((r, i) => r.GetBoolean(i)) },
            { typeof(int), new Func<SqlDataReader, int, int>((r, i) => r.GetInt32(i)) },
            { typeof(long), new Func<SqlDataReader, int, long>((r, i) => r.GetInt64(i)) },
            { typeof(decimal), new Func<SqlDataReader, int, decimal>((r, i) => r.GetDecimal(i)) },
        };

        /// <summary>
        /// Gets the value of the field at the specified index with
        /// minimal allocations at the expense of some CPU cycles.
        /// </summary>
        public static T GetValueOrDefaultAlt<T>(this SqlDataReader reader, string name)
        {
            int index = reader.GetOrdinal(name);

            return reader.GetValueOrDefaultAlt<T>(index);
        }

        /// <summary>
        /// Gets the value of the field at the specified index with
        /// minimal allocations at the expense of some CPU cycles.
        /// </summary>
        public static T GetValueOrDefaultAlt<T>(this SqlDataReader reader, int index)
        {
            if (reader.IsDBNull(index)) {
                return default(T);
            }

            Type fieldType = reader.GetFieldType(index);
            object delegateObj;

            // Lookup + cast may seem slow but it's better than boxing / GC pressure.
            if (Delegates.TryGetValue(fieldType, out delegateObj)) {
                return ((Func<SqlDataReader, int, T>)delegateObj).Invoke(reader, index);
            }

            return (T)reader[index];
        }
    }
}