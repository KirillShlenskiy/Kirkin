using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Kirkin
{
    /// <summary>
    /// General-purpose 64-bit variant value container.
    /// Avoids boxing for common value types.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Variant2
    {
        private static readonly object s_useStore = new object();

        // Used to encode System.Type values.
        private class ClrType<T> { };

        // Variant storage.
        [FieldOffset(0)]
        private long Store;

        [FieldOffset(8)]
        private object BoxedValue;

        /// <summary>
        /// Gets the type of the underlying value.
        /// </summary>
        public Type ValueType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal int Int32
        {
            get
            {
                return GetValue<int>();
            }
        }

        internal long Int64
        {
            get
            {
                return GetValue<long>();
            }
        }

        internal float Single
        {
            get
            {
                return GetValue<float>();
            }
        }

        internal double Double
        {
            get
            {
                return GetValue<double>();
            }
        }

        public Variant2(int value)
        {
            BoxedValue = s_useStore;

            fixed (long* store = &Store)
            {
                int* myBytes = (int*)store;

                *(myBytes) = value;
            }
        }

        //public Variant2(long value)
        //{
        //    FastStore = (IntPtr)(&value);
        //    BoxedValue = null;
        //}

        //public Variant2(float value)
        //{
        //    FastStore = (IntPtr)(&value);
        //    BoxedValue = null;
        //}

        //public Variant2(double value)
        //{
        //    FastStore = (IntPtr)(&value);
        //    BoxedValue = null;
        //}

        //public Variant2(DateTime value)
        //{
        //    FastStore = *(IntPtr*)&value;
        //    BoxedValue = null;
        //}

        /// <summary>
        /// Boxing constructor.
        /// </summary>
        public Variant2(object value)
        {
            Store = 0;
            BoxedValue = value;
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
            if (BoxedValue == s_useStore)
            {
                fixed (long* store = &Store)
                {
                    byte* bytePtr = (byte*)store;
                    T result = default(T);
                    TypedReference resultRef = __makeref(result);
                    byte* resultPtr = (byte*)*((IntPtr*)&resultRef);

                    for (int i = 0; i < sizeof(long); ++i) {
                        resultPtr[i] = bytePtr[i];
                    }

                    return result;
                }
            }

            return (T)BoxedValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void WriteGenericToPtr<T>(IntPtr dest, T value, int sizeOfT) where T : struct
        {
            byte* bytePtr = (byte*)dest;

            TypedReference valueref = __makeref(value);
            byte* valuePtr = (byte*)*((IntPtr*)&valueref);

            for (int i = 0; i < sizeOfT; ++i)
            {
                bytePtr[i] = valuePtr[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T ReadGenericFromPtr<T>(IntPtr source, int sizeOfT)
        {
            byte* bytePtr = (byte*)source;
            T result = default(T);
            TypedReference resultRef = __makeref(result);
            byte* resultPtr = (byte*)*((IntPtr*)&resultRef);

            for (int i = 0; i < sizeOfT; ++i) {
                resultPtr[i] = bytePtr[i];
            }

            return result;
        }
    }
}