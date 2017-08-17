using System.Collections;
using System.Collections.Generic;

namespace Kirkin.Data
{
    internal interface IColumnStorage
    {
        int Count { get; }
        int Capacity { get; set; }

        object Get(int index);
        void Set(int index, object value);
    }

    internal abstract class ColumnStorage<T> : IColumnStorage
    {
        private readonly BitArray _dbNullBits;
        public abstract int Count { get; }
        public abstract int Capacity { get; set; }

        public abstract T Get(int index);
        public abstract void Set(int index, T value);

        object IColumnStorage.Get(int index)
        {
            return Get(index);
        }

        void IColumnStorage.Set(int index, object value)
        {
            Set(index, (T)value);
        }
    }

    internal sealed class ObjectColumnStorage : ColumnStorage<object>
    {
        private readonly List<object> _store = new List<object>();

        public override int Count
        {
            get
            {
                return _store.Count;
            }
        }

        public override int Capacity
        {
            get
            {
                return _store.Capacity;
            }
            set
            {
                _store.Capacity = value;
            }
        }

        public override object Get(int index)
        {
            return _store[index];
        }

        public override void Set(int index, object value)
        {
            _store[index] = value;
        }
    }
}