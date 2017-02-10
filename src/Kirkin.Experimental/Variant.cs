using System;
using System.Runtime.InteropServices;

namespace Kirkin
{
    /// <summary>
    /// General-purpose 128-bit variant value container.
    /// Avoids boxing for common value types.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Variant
    {
        private class ClrType<T> { };

        // Stores the Type of the value if the value is of a
        // known primitive type, or the value itself otherwise.
        [FieldOffset(0)]
        public object TypeOrBoxedValue;

        // Variant storage.
        [FieldOffset(4)] public int Int32;
        [FieldOffset(4)] public long Int64;

        public Variant(int value)
        {
            Int64 = 0;
            TypeOrBoxedValue = typeof(int);
            Int32 = value;
        }

        public Variant(long value)
        {
            Int32 = 0;
            TypeOrBoxedValue = typeof(long);
            Int64 = value;
        }

        /// <summary>
        /// Boxing constructor.
        /// </summary>
        public Variant(object value)
        {
            Int32 = 0;
            Int64 = 0;

            if (value != null && value is Type)
            {
                // Special case.
                TypeOrBoxedValue = typeof(ClrType<>).MakeGenericType((Type)value);
            }
            else
            {
                TypeOrBoxedValue = value;
            }
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

                throw new InvalidCastException();
            }

            return ValueResolver<T>.Func(this);
        }

        static class ValueResolver<T>
        {
            internal static readonly Func<Variant, T> Func = (Func<Variant, T>)WellKnownValueTypeDelegate();

            private static object WellKnownValueTypeDelegate()
            {
                Type type = typeof(T);

                if (type == typeof(int)) return new Func<Variant, int>(v => v.Int32);
                if (type == typeof(long)) return new Func<Variant, long>(v => v.Int64);

                throw new InvalidOperationException($"Unknown type: {type}.");
            }
        }
    }
}