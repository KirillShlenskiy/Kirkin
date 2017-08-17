using System;
using System.Collections;

namespace Kirkin.Data
{
    internal sealed class ColumnStorage<T> : IColumnStorage
    {
        private BitArray _dbNullBits;
        private T[] _store;

        public int Capacity
        {
            get
            {
                return _dbNullBits.Count;
            }
            set
            {
                BitArray newDbNullBits = new BitArray(value);

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
            return _store[index];
        }

        public void Set(int index, T value)
        {
            _store[index] = value;
        }

        private void SetCapacity(int capacity)
        {
            Array.Resize(ref _store, capacity);
        }

        public bool IsNull(int index)
        {
            return _dbNullBits[index];
        }

        object IColumnStorage.Get(int index)
        {
            if (IsNull(index)) {
                return DBNull.Value;
            }

            return Get(index);
        }

        void IColumnStorage.Set(int index, object value)
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