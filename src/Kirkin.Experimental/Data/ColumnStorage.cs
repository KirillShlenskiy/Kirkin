using System;
using System.Collections;

namespace Kirkin.Data
{
    internal interface IColumnStorage
    {
        int Capacity { get; set; }

        bool IsNull(int index);
        object Get(int index);
        void Set(int index, object value);
    }

    internal abstract class ColumnStorage<T> : IColumnStorage
    {
        private BitArray _dbNullBits;

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

        public abstract T Get(int index);
        public abstract void Set(int index, T value);
        protected abstract void SetCapacity(int capacity);

        public bool IsNull(int index)
        {
            return _dbNullBits[index];
        }

        object IColumnStorage.Get(int index)
        {
            if (_dbNullBits[index]) {
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

    internal sealed class ArrayColumnStorage<T> : ColumnStorage<T>
    {
        private T[] _store;

        public override T Get(int index)
        {
            return _store[index];
        }

        public override void Set(int index, T value)
        {
            _store[index] = value;
        }

        protected override void SetCapacity(int capacity)
        {
            Array.Resize(ref _store, capacity);
        }
    }
}