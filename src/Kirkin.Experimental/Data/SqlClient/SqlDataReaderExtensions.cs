#if !__MOBILE__

using System;
using System.Data.SqlClient;

namespace Kirkin.Data.SqlClient
{
    /// <summary>
    /// <see cref="SqlDataReader"/> extension methods.
    /// </summary>
    public static class SqlDataReaderExtensions
    {
        /// <summary>
        /// Gets the value of the field at the specified index. Tuned to minimize the
        /// amount of boxing performed on well-known value types i.e. int, decimal etc.
        /// </summary>
        public static T GetValueOrDefault<T>(this SqlDataReader reader, string name)
        {
            int index = reader.GetOrdinal(name);

            return reader.GetValueOrDefault<T>(index);
        }

        /// <summary>
        /// Gets the value of the field at the specified index. Tuned to minimize the
        /// amount of boxing performed on well-known value types i.e. int, decimal etc.
        /// </summary>
        public static T GetValueOrDefault<T>(this SqlDataReader reader, string name, T defaultValue)
        {
            int index = reader.GetOrdinal(name);

            return reader.GetValueOrDefault(index, defaultValue);
        }

        /// <summary>
        /// Gets the value of the field at the specified index. Tuned to minimize the
        /// amount of boxing performed on well-known value types i.e. int, decimal etc.
        /// </summary>
        public static T GetValueOrDefault<T>(this SqlDataReader reader, int index)
        {
            return reader.IsDBNull(index)
                ? default(T)
                : GetValueDelegateResolver<T>.GetValueDelegate(reader, index);
        }

        /// <summary>
        /// Gets the value of the field at the specified index. Tuned to minimize the
        /// amount of boxing performed on well-known value types i.e. int, decimal etc.
        /// </summary>
        public static T GetValueOrDefault<T>(this SqlDataReader reader, int index, T defaultValue)
        {
            return reader.IsDBNull(index)
                ? defaultValue
                : GetValueDelegateResolver<T>.GetValueDelegate(reader, index);
        }

        // Per-type GetValue delegate cache.
        static class GetValueDelegateResolver<T>
        {
            public static readonly Func<SqlDataReader, int, T> GetValueDelegate = CreateDelegate();

            private static Func<SqlDataReader, int, T> CreateDelegate()
            {
                // Try to resolve a well-known delegate for this return type.
                object wellKnownGetValueDelegate = WellKnownGetValueDelegates.ForFieldType(typeof(T));

                return (wellKnownGetValueDelegate != null)
                    ? (Func<SqlDataReader, int, T>)wellKnownGetValueDelegate
                    : (reader, index) => (T)reader.GetValue(index); // Default GetValue implementation.
            }
        }

        static class WellKnownGetValueDelegates
        {
            // Caching to reduce allocations.
            private static readonly Func<SqlDataReader, int, bool> Boolean = (r, i) => r.GetBoolean(i);
            private static readonly Func<SqlDataReader, int, DateTime> DateTime = (r, i) => r.GetDateTime(i);
            private static readonly Func<SqlDataReader, int, decimal> Decimal = (r, i) => r.GetDecimal(i);
            private static readonly Func<SqlDataReader, int, double> Double = (r, i) => r.GetDouble(i);
            private static readonly Func<SqlDataReader, int, float> Float = (r, i) => r.GetFloat(i);
            private static readonly Func<SqlDataReader, int, int> Int32 = (r, i) => r.GetInt32(i);
            private static readonly Func<SqlDataReader, int, long> Int64 = (r, i) => r.GetInt64(i);

            public static object ForFieldType(Type fieldType)
            {
                if (fieldType == typeof(int)) return Int32;
                if (fieldType == typeof(bool)) return Boolean;
                if (fieldType == typeof(decimal)) return Decimal;
                if (fieldType == typeof(DateTime)) return DateTime;
                if (fieldType == typeof(long)) return Int64;
                if (fieldType == typeof(double)) return Double;
                if (fieldType == typeof(float)) return Float;

                return null;
            }
        }
    }
}

#endif