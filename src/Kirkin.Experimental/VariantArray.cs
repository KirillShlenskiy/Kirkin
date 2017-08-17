using System;

namespace Kirkin
{
    internal unsafe struct VariantArray
    {
        private readonly int[] _store;

        public VariantArray(int sizeBytes)
        {
            if (sizeBytes % 4 != 0) throw new ArgumentException("Size must be divisible by 4");

            _store = new int[sizeBytes / 4];
        }

        public void SetInt(int offset, int value)
        {
            fixed (int* s = _store) {
                *(s + offset / 4) = value;
            }
        }

        public void SetLong(int offset, long value)
        {
            fixed (int* s = _store){
                *((long*)s + offset / 4) = value;
            }
        }

        public int GetInt(int offset)
        {
            fixed (int* s = _store)
            {
                int copy = *(s + offset / 4);

                return copy;
            }
        }

        public long GetLong(int offset)
        {
            fixed (int* s = _store)
            {
                long copy = *((long*)s + offset / 4);

                return copy;
            }
        }
    }
}