using System;
using System.Buffers;

namespace Kirkin.Collections.Generic
{
    /// <summary>
    /// Mutable **value** type which provides basic array
    /// builder functionality with minimal allocations.
    /// This is a mutable struct. Use with caution and avoid copy.
    /// </summary>
    internal struct PooledArrayBuilder<T>
    {
        private const int DefaultCapacity = 8;
        private static readonly ArrayPool<T> BufferPool = ArrayPool<T>.Shared;

        private T[] items;
        private int count;

        public PooledArrayBuilder(int capacity)
        {
            items = BufferPool.Rent(capacity);
            count = 0;
        }

        /// <summary>
        /// Gets the number of items already in the builder.
        /// </summary>
        public int Count
        {
            get
            {
                return count;
            }
        }

        /// <summary>
        /// Adds the given item to the builder.
        /// Causes the builder to expand if the capacity is exceeded.
        /// Use when the number of items is not known
        /// in advance and is expected to be small.
        /// </summary>
        public void Add(T item)
        {
            if (items == null)
            {
                items = BufferPool.Rent(DefaultCapacity);
            }
            else if (items.Length == count)
            {
                Grow();
            }

            items[count++] = item;
        }

        /// <summary>
        /// Doubles the size of the underlying array.
        /// </summary>
        private void Grow()
        {
            T[] newItems = new T[count * 2];

            Array.Copy(items, 0, newItems, 0, count);
            BufferPool.Return(items);

            items = newItems;
        }

        /// <summary>
        /// Adds the given item to the builder.
        /// Throws if the underlying array is not
        /// initialized or its capacity exceeded.
        /// </summary>
        public void FastAdd(T item)
        {
            items[count++] = item;
        }

        /// <summary>
        /// Materializes the array.
        /// </summary>
        public T[] ToArray()
        {
            if (count == 0) return Array<T>.Empty;
            //if (items.Length == count && !BufferPool.MayNeedReturning(items)) return items;

            return ToArraySlow();
        }

        /// <summary>
        /// Materializes the array.
        /// </summary>
        private T[] ToArraySlow()
        {
            T[] result = new T[count];

            Array.Copy(items, 0, result, 0, count);
            BufferPool.Return(items);

            return result;
        }

        /// <summary>
        /// Wraps the array with an immutable <see cref="Vector{T}"/> facade.
        /// </summary>
        public Vector<T> ToVector()
        {
            return new Vector<T>(ToArray());
        }
    }
}