using System;
using System.Runtime.InteropServices;

namespace Kirkin
{
    /// <summary>
    /// General-purpose 64-bit variant value container.
    /// Avoids boxing for common value types.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Variant
    {
        // Used to encode System.Type values.
        private class ClrType<T> { };

        // Stores the Type of the value if the value is of a
        // known primitive type, or the value itself otherwise.
        [FieldOffset(0)]
        private readonly object TypeOrBoxedValue;

        // Variant storage.
        [FieldOffset(4)] private readonly int Int32;
        [FieldOffset(4)] private readonly long Int64;
        [FieldOffset(4)] private readonly float Float;
        [FieldOffset(4)] private readonly double Double;
        [FieldOffset(4)] private readonly DateTime DateTime;

        /// <summary>
        /// Gets the type of the underlying value.
        /// </summary>
        public Type ValueType
        {
            get
            {
                Type type = TypeOrBoxedValue as Type;

                if (type != null)
                {
                    return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ClrType<>))
                        ? typeof(Type) // Special case.
                        : type;
                }

                return TypeOrBoxedValue == null
                    ? typeof(object)
                    : TypeOrBoxedValue.GetType();
            }
        }

        public Variant(int value)
        {
            TypeOrBoxedValue = typeof(int);
            Int64 = 0;
            Float = 0;
            Double = 0;
            DateTime = default(DateTime);
            Int32 = value;
        }

        public Variant(long value)
        {
            TypeOrBoxedValue = typeof(long);
            Int32 = 0;
            Float = 0;
            Double = 0;
            DateTime = default(DateTime);
            Int64 = value;
        }

        public Variant(float value)
        {
            TypeOrBoxedValue = typeof(float);
            Int32 = 0;
            Int64 = 0;
            Double = 0;
            DateTime = default(DateTime);
            Float = value;
        }

        public Variant(double value)
        {
            TypeOrBoxedValue = typeof(double);
            Int32 = 0;
            Int64 = 0;
            Float = 0;
            DateTime = default(DateTime);
            Double = value;
        }

        public Variant(DateTime value)
        {
            TypeOrBoxedValue = typeof(DateTime);
            Int32 = 0;
            Int64 = 0;
            Float = 0;
            Double = 0;
            DateTime = value;
        }

        /// <summary>
        /// Boxing constructor.
        /// </summary>
        public Variant(object value)
        {
            if (value != null && value is Type)
            {
                // Special case.
                TypeOrBoxedValue = typeof(ClrType<>).MakeGenericType((Type)value);
            }
            else
            {
                TypeOrBoxedValue = value;
            }

            Int32 = 0;
            Int64 = 0;
            Float = 0;
            Double = 0;
            DateTime = default(DateTime);
        }

        ///// <summary>
        ///// Gets the underlying value boxing it if necessary.
        ///// </summary>
        //public object GetValue()
        //{

        //}

        /// <summary>
        /// Gets the underlying value or throws if it's not of the given type.
        /// </summary>
        public T GetValue<T>()
        {
            if (ReferenceEquals(typeof(T), TypeOrBoxedValue) && ValueResolver<T>.Func != null) {
                return ValueResolver<T>.Func(this);
            }

            return GetValueSlow<T>();
        }

        private T GetValueSlow<T>()
        {
            Type type = TypeOrBoxedValue as Type;

            if (type == null) {
                return (T)TypeOrBoxedValue;
            }

            if (typeof(T) != type)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ClrType<>))
                {
                    // Special case.
                    return (T)(object)type.GenericTypeArguments[0];
                }
            }

            throw new InvalidCastException();
        }

        static class ValueResolver<T>
        {
            internal static readonly Func<Variant, T> Func = (Func<Variant, T>)WellKnownValueTypeDelegate();

            private static object WellKnownValueTypeDelegate()
            {
                Type type = typeof(T);

                if (type == typeof(int)) return new Func<Variant, int>(v => v.Int32);
                if (type == typeof(long)) return new Func<Variant, long>(v => v.Int64);
                if (type == typeof(float)) return new Func<Variant, float>(v => v.Float);
                if (type == typeof(double)) return new Func<Variant, double>(v => v.Double);
                if (type == typeof(DateTime)) return new Func<Variant, DateTime>(v => v.DateTime);

                return null;
            }
        }
    }
}