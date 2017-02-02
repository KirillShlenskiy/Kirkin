using System;

namespace Kirkin.Collections.Generic
{
    /// <summary>
    /// Mutable **value** type which provides basic array
    /// builder functionality with minimal allocations.
    /// This is a mutable struct. Use with caution and avoid copy.
    /// </summary>
    /// <remarks>
    /// Usage guidelines:
    /// - Known number of elements, high perf requirement: use constructor which specifies capacity and UnsafeAdd.
    /// - Unknown number of elements, high perf requirement: use default constructor and Add. Still faster than <see cref="System.Collections.Generic.List{T}"/>.
    /// - Short-circuiting <see cref="System.Collections.Generic.IEnumerable{T}"/> required: do not use <see cref="ArrayBuilder{T}"/>. Use yield.
    /// - Rich array builder functionality required: use <see cref="System.Collections.Generic.List{T}"/>.
    /// - Extreme perf essential: do not use <see cref="ArrayBuilder{T}"/>. Use T[], int counter and <see cref="Array.Resize{T}(ref T[], int)"/> as needed.
    /// </remarks>
    public struct ArrayBuilder<T>
    {
        private const int DefaultCapacity = 8;

        private T[] items;
        private int count;

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
        /// Creates an array builder with the given capacity.
        /// </summary>
        public ArrayBuilder(int capacity)
        {
            items = new T[capacity];
            count = 0;
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
                items = new T[DefaultCapacity];
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
        /// Resets the count without clearing or swapping out the underlying array.
        /// Will corrupt arrays returned by previous calls to <see cref="ToArray"/>
        /// that did not result in a full array copy.
        /// </summary>
        internal void FastClear()
        {
            count = 0;
        }

        /// <summary>
        /// Materializes the array. Returns the underlying buffer
        /// instance without copying if capacity = count.
        /// </summary>
        public T[] ToArray()
        {
            if (count == 0) return Array<T>.Empty;

            // Returning underlying buffer. Be careful not
            // to use in conjunction with FastClear().
            if (items.Length == count) return items;

            return ToArraySlow();
        }

        /// <summary>
        /// Materializes the array. Creates a copy of the underlying buffer in all cases
        /// (excep when count = 0). Safe to use in conjunction with <see cref="FastClear()"/>.
        /// </summary>
        internal T[] ToArrayCopy()
        {
            if (count == 0) return Array<T>.Empty;

            return ToArraySlow();
        }

        /// <summary>
        /// Materializes the array.
        /// </summary>
        private T[] ToArraySlow()
        {
            T[] result = new T[count];

            Array.Copy(items, 0, result, 0, count);

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