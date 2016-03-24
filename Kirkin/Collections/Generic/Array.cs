using System;

namespace Kirkin.Collections.Generic
{
    /// <summary>
    /// Common utility methods for working with generic arrays.
    /// </summary>
    public static class Array<T>
    {
        /// <summary>
        /// Shared empty array instance.
        /// </summary>
        public static readonly T[] Empty = new T[0];

        // Usage guidelines:
        //
        // - Known number of elements, high perf requirement: use constructor which specifies capacity and UnsafeAdd.
        // - Unknown number of elements, high perf requirement: use default constructor and Add. Still faster than System.Collections.Generic.List<T>.
        // - Short-circuiting System.Collections.Generic.IEnumerable<T> required: do not use ArrayBuilder<T>. Use yield.
        // - Rich array builder functionality required: use System.Collections.Generic.List<T>.
        // - Extreme perf essential: do not use ArrayBuilder<T>. Use T[], int counter and Array.Resize<T> as needed.

        /// <summary>
        /// Mutable **value** type which provides basic array
        /// builder functionality with minimal allocations.
        /// This is a mutable struct. Use with caution and avoid copy.
        /// </summary>
        public struct Builder
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
            public Builder(int capacity)
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
            /// initialised or its capacity exceeded.
            /// </summary>
            public void UnsafeAdd(T item)
            {
                items[count++] = item;
            }

            /// <summary>
            /// Materialises the array.
            /// </summary>
            public T[] ToArray()
            {
                if (count == 0) return Empty;
                if (items.Length == count) return items;

                return ToArraySlow();
            }

            /// <summary>
            /// Materialises the array.
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
}