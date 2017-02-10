using System;
using System.Runtime.CompilerServices;

namespace Kirkin
{
    /// <summary>
    /// General-purpose 64-bit variant value container.
    /// Avoids boxing for common value types.
    /// </summary>
    public unsafe struct Variant2
    {
        // Used to encode System.Type values.
        private class ClrType<T> { };

        // Variant storage.
        private IntPtr FastStore;

        private object BoxedValue;

        /// <summary>
        /// Gets the type of the underlying value.
        /// </summary>
        public Type ValueType
        {
            get
            {
                return __reftype(__makeref(FastStore));
            }
        }

        public Variant2(int value)
        {
            TypedReference tr = __makeref(value);

            FastStore = (IntPtr)(&tr);
            BoxedValue = null;
        }

        public Variant2(long value)
        {
            FastStore = (IntPtr)(&value);
            BoxedValue = null;
        }

        public Variant2(float value)
        {
            FastStore = (IntPtr)(&value);
            BoxedValue = null;
        }

        public Variant2(double value)
        {
            FastStore = (IntPtr)(&value);
            BoxedValue = null;
        }

        public Variant2(DateTime value)
        {
            FastStore = *(IntPtr*)&value;
            BoxedValue = null;
        }

        /// <summary>
        /// Boxing constructor.
        /// </summary>
        public Variant2(object value)
        {
            FastStore = IntPtr.Zero;
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
            if (FastStore != IntPtr.Zero) {
                return ReadGenericFromPtr<T>(FastStore, sizeof(int));
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