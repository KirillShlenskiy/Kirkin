#if !__MOBILE__

using System;
using System.Data;

namespace Kirkin.Data
{
    /// <summary>
    /// <see cref="IDataReader"/> extension methods.
    /// </summary>
    internal static class DataRecordExtensions
    {
        /// <summary>
        /// Returns the value of the field with the given name, or default value
        /// of type <typeparam name="T" /> if the field is null. Tuned to minimize the
        /// amount of boxing performed on well-known value types i.e. int, decimal etc.
        /// </summary>
        public static T GetValueOrDefault<T>(this IDataRecord reader, string name)
        {
            int index = reader.GetOrdinal(name);

            return reader.GetValueOrDefault<T>(index);
        }

        /// <summary>
        /// Returns the value of the field with the given name, or the
        /// given default value if the field is null. Tuned to minimize the
        /// amount of boxing performed on well-known value types i.e. int, decimal etc.
        /// </summary>
        public static T GetValueOrDefault<T>(this IDataRecord reader, string name, T defaultValue)
        {
            int index = reader.GetOrdinal(name);

            return reader.GetValueOrDefault(index, defaultValue);
        }

        /// <summary>
        /// Returns the value of the field at the specified index, or default value
        /// of type <typeparam name="T" /> if the field is null. Tuned to minimize the
        /// amount of boxing performed on well-known value types i.e. int, decimal etc.
        /// </summary>
        public static T GetValueOrDefault<T>(this IDataRecord reader, int index)
        {
            return reader.IsDBNull(index)
                ? default(T)
                : GetValueDelegateVariant<T>.Func(reader, index);
        }

        /// <summary>
        /// Returns the value of the field at the specified index, or the
        /// given default value if the field is null. Tuned to minimize the
        /// amount of boxing performed on well-known value types i.e. int, decimal etc.
        /// </summary>
        public static T GetValueOrDefault<T>(this IDataRecord reader, int index, T defaultValue)
        {
            return reader.IsDBNull(index)
                ? defaultValue
                : GetValueDelegateVariant<T>.Func(reader, index);
        }

        static class GetValueDelegateVariant<T>
        {
            // Per-type GetValue delegate cache.
            internal static readonly Func<IDataRecord, int, T> Func
                = (Func<IDataRecord, int, T>)WellKnownDelegateForFieldType(typeof(T)) ?? GetValueDefault;

            private static T GetValueDefault(IDataRecord reader, int index)
            {
                return (T)reader.GetValue(index);
            }
        }

        static object WellKnownDelegateForFieldType(Type fieldType)
        {
            // Well-known GetValue delegates.
            if (fieldType == typeof(int)) return new Func<IDataRecord, int, int>((r, i) => r.GetInt32(i));
            if (fieldType == typeof(bool)) return new Func<IDataRecord, int, bool>((r, i) => r.GetBoolean(i));
            if (fieldType == typeof(decimal)) return new Func<IDataRecord, int, decimal>((r, i) => r.GetDecimal(i));
            if (fieldType == typeof(DateTime)) return new Func<IDataRecord, int, DateTime>((r, i) => r.GetDateTime(i));
            if (fieldType == typeof(long)) return new Func<IDataRecord, int, long>((r, i) => r.GetInt64(i));
            if (fieldType == typeof(double)) return new Func<IDataRecord, int, double>((r, i) => r.GetDouble(i));
            if (fieldType == typeof(float)) return new Func<IDataRecord, int, float>((r, i) => r.GetFloat(i));

            return null;
        }
    }
}

#endif