using System;

namespace Kirkin.ValueConversion
{
    /// <summary>
    /// Performs basic value type conversions.
    /// </summary>
    public class ValueConverter : ValueConverterBase
    {
        /// <summary>
        /// Singleton <see cref="ValueConverter"/> instance.
        /// </summary>
        public static ValueConverter Default { get; } = new ValueConverter();

        protected ValueConverter()
        {
        }

        /// <summary>
        /// Converts the value to the given type.
        /// Returns T if the value can be cast.
        /// Returns default(T) if the value is DBNull.
        /// Falls back to using <see cref="Convert.ChangeType(object, Type)"/> otherwise.
        /// </summary>
        public override T Convert<T>(object value)
        {
            if (value is T) return (T)value;
            if (value == null) return default(T);
            if (value is DBNull) return default(T);

            return (T)System.Convert.ChangeType(value, typeof(T));
        }
    }
}