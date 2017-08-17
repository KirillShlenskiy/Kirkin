using System;
using System.Collections;

namespace Kirkin.Data
{
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

                SetCapacity(value);

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

        private void SetCapacity(int capacity)
        {
            Array.Resize(ref _array, capacity);
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