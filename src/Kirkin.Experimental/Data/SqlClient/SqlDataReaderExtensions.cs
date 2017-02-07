using System;
using System.Data.SqlClient;

namespace Kirkin.Data.SqlClient
{
    public static class SqlDataReaderExtensions
    {
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
            Type fieldType = reader.GetFieldType(index);

            if (fieldType.IsValueType)
            {
                // Try get well-known type.
                if (reader.IsDBNull(index)) {
                    return default(T);
                }

                object delegateObj = GetValueOrDefaultDelegate(fieldType);

                if (delegateObj != null) {
                    return ((Func<SqlDataReader, int, T>)delegateObj).Invoke(reader, index);
                }
            }

            return DefaultIfDbNull<T>(reader.GetValue(index));
        }

        private static T DefaultIfDbNull<T>(object value)
        {
            return value is DBNull ? default(T) : (T)value;
        }

        private static object GetValueOrDefaultDelegate(Type fieldType)
        {
            if (fieldType == typeof(int)) return new Func<SqlDataReader, int, int>((r, i) => r.GetInt32(i));
            if (fieldType == typeof(bool)) return new Func<SqlDataReader, int, bool>((r, i) => r.GetBoolean(i));
            if (fieldType == typeof(decimal)) return new Func<SqlDataReader, int, decimal>((r, i) => r.GetDecimal(i));
            if (fieldType == typeof(long)) return new Func<SqlDataReader, int, long>((r, i) => r.GetInt64(i));

            return null;
        }
    }
}