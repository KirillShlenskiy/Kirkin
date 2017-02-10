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
        // Storage.
        [FieldOffset(0)] public int Int32;
        [FieldOffset(0)] public long Int64;

        // Stores the Type of the value if the value is of a
        // known primitive type, or the value itself otherwise.
        [FieldOffset(1)] public object TypeOrBoxedValue;

        public Variant(int value)
        {
            Int64 = 0;
            TypeOrBoxedValue = typeof(int);
            Int32 = value;
        }

        public Variant(long value)
        {
            Int32 = 0;
            TypeOrBoxedValue = typeof(int);
            Int64 = value;
        }

        /// <summary>
        /// Boxing constructor.
        /// </summary>
        public Variant(object value)
        {
            Int32 = 0;
            Int64 = 0;
            TypeOrBoxedValue = value;
        }

        /// <summary>
        /// Gets the underlying value or throws if it's not of the given type.
        /// </summary>
        public T GetValue<T>()
        {
            Type type = TypeOrBoxedValue as Type;

            if (type != null && type != typeof(T)) {
                throw new InvalidCastException();
            }

            return ValueResolver<T>.Func(this); // ref?
        }

        static class ValueResolver<T>
        {
            internal static readonly Func<Variant, T> Func = (Func<Variant, T>)CreateDelegate()
                ?? new Func<Variant, T>(v => (T)v.TypeOrBoxedValue);

            private static object CreateDelegate()
            {
                Type type = typeof(T);

                if (type == typeof(int)) return new Func<Variant, int>(v => v.Int32);
                if (type == typeof(long)) return new Func<Variant, long>(v => v.Int64);

                return null;
            }
        }
    }
}