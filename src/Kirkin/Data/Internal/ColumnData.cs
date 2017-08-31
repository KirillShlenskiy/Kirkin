using System;
using System.Collections;

namespace Kirkin.Data.Internal
{
    /// <summary>
    /// Column data container implementation.
    /// </summary>
    internal sealed class ColumnData<T> : IColumnData
    {
        private BitArray _dbNullBits;
        private T[] _array;

        public int Capacity
        {
            get
            {
                return _dbNullBits.Count;
            }
            set
            {
                // Default all null bits to true in order to fill uninitialized rows with DBNulls.
                BitArray newDbNullBits = new BitArray(value, defaultValue: true);

                if (_dbNullBits != null)
                {
                    for (int i = 0; i < _dbNullBits.Count && i < newDbNullBits.Count; i++) {
                        newDbNullBits[i] = _dbNullBits[i];
                    }
                }

                Array.Resize(ref _array, value);

                _dbNullBits = newDbNullBits;
            }
        }

        public T Get(int index)
        {
            return _array[index];
        }

        public void Set(int index, T value)
        {
            _array[index] = value;
        }

        public void Remove(int index)
        {
            for (int i = index + 1; i < _dbNullBits.Length; i++) {
                _dbNullBits[i - 1] = _dbNullBits[i];
            }

            for (int i = index + 1; i < _array.Length; i++) {
                _array[i - 1] = _array[i];
            }
        }

        public bool IsNull(int index)
        {
            return _dbNullBits[index];
        }

        object IColumnData.Get(int index)
        {
            if (IsNull(index)) {
                return DBNull.Value;
            }

            return Get(index);
        }

        void IColumnData.Set(int index, object value)
        {
            if (value is DBNull)
            {
                _dbNullBits[index] = true;
                Set(index, default(T));
            }
            else
            {
                _dbNullBits[index] = false;
                Set(index, (T)value);
            }
        }
    }
}