﻿using System;
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
            if (reader.IsDBNull(index)) {
                return default(T);
            }

            Type fieldType = typeof(T);

            if (fieldType.IsValueType)
            {
                // Try resolve well-known delegate for this return type.
                object func = WellKnownGetValueDelegates.Find(fieldType);

                if (func != null) {
                    return ((Func<SqlDataReader, int, T>)func).Invoke(reader, index);
                }
            }

            return (T)reader.GetValue(index);
        }

        static class WellKnownGetValueDelegates
        {
            private static readonly Func<SqlDataReader, int, bool> Boolean = (r, i) => r.GetBoolean(i);
            private static readonly Func<SqlDataReader, int, DateTime> DateTime = (r, i) => r.GetDateTime(i);
            private static readonly Func<SqlDataReader, int, decimal> Decimal = (r, i) => r.GetDecimal(i);
            private static readonly Func<SqlDataReader, int, double> Double = (r, i) => r.GetDouble(i);
            private static readonly Func<SqlDataReader, int, float> Float = (r, i) => r.GetFloat(i);
            private static readonly Func<SqlDataReader, int, int> Int32 = (r, i) => r.GetInt32(i);
            private static readonly Func<SqlDataReader, int, long> Int64 = (r, i) => r.GetInt64(i);

            public static object Find(Type fieldType)
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