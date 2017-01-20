using System;

using Kirkin.ValueConversion;

using NUnit.Framework;

namespace Kirkin.Tests.ValueConversion
{
    public sealed class ValueConverterTests
    {
        static ValueConverterTests()
        {
            ValueConverter.Default.Convert<int>(null);
        }

        [Test]
        public void BasicConversion()
        {
            Assert.AreEqual(123, ValueConverter.Default.Convert<int>("123"));
            Assert.AreEqual(123, ValueConverter.Default.Convert("123", typeof(int)));
        }

        [Test]
        public void PerfValue()
        {
            object value = 123;

            for (int i = 0; i < 10000000; i++) {
                ValueConverter.Default.Convert<int>(value);
            }
        }

        [Test]
        public void PerfValueNonGeneric()
        {
            object value = 123;

            for (int i = 0; i < 10000000; i++) {
                ValueConverter.Default.Convert(value, typeof(int));
            }
        }

        [Test]
        public void PerfValueStatic()
        {
            object value = 123;

            for (int i = 0; i < 10000000; i++) {
                StaticConverter.Convert<int>(value);
            }
        }

        [Test]
        public void PerfNull()
        {
            object value = null;

            for (int i = 0; i < 10000000; i++) {
                ValueConverter.Default.Convert<int>(value);
            }
        }

        [Test]
        public void PerfDbNull()
        {
            object value = DBNull.Value;

            for (int i = 0; i < 10000000; i++) {
                ValueConverter.Default.Convert<int>(value);
            }
        }

        /// <summary>
        /// Performs basic value type conversions.
        /// </summary>
        public static class StaticConverter
        {
            /// <summary>
            /// Converts the value to the given type.
            /// Returns T if the value can be cast.
            /// Returns default(T) if the value is DBNull.
            /// Falls back to using <see cref="Convert.ChangeType(object, Type)"/> otherwise.
            /// </summary>
            public static T Convert<T>(object value)
            {
                if (value is T) return (T)value;

                return SlowConverter<T>.Convert(value);
            }

            static class SlowConverter<T>
            {
                public static T Convert(object value)
                {
                    if (value == null) return default(T);
                    if (value is DBNull) return default(T);

                    return (T)System.Convert.ChangeType(value, typeof(T));
                }
            }
        }
    }
}