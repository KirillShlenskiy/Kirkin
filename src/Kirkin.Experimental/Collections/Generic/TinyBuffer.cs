namespace Kirkin.Collections.Generic
{
    /// <summary>
    /// Small linked list-backed value type buffer which is optimized for
    /// building very small (10 items or less) arrays of unknown capacity.
    /// </summary>
    internal struct TinyBuffer<T>
    {
        sealed class Node
        {
            internal int Depth;
            internal T Value;
            internal Node Previous;
        }

        private Node Tail;

        /// <summary>
        /// Gets the number of items this builder currently contains.
        /// </summary>
        public int Count
        {
            get
            {
                return Tail == null ? 0 : Tail.Depth + 1;
            }
        }

        /// <summary>
        /// Adds the given item to the builder.
        /// </summary>
        public void Add(T item)
        {
            Tail = new Node {
                Depth = Count,
                Value = item,
                Previous = Tail
            };
        }

        /// <summary>
        /// Materializes the array.
        /// </summary>
        public T[] ToArray()
        {
            if (Tail == null) return Array<T>.Empty;

            return ToArraySlow();
        }

        /// <summary>
        /// Materializes the array.
        /// </summary>
        private T[] ToArraySlow()
        {
            T[] array = new T[Tail.Depth + 1];

            for (Node node = Tail; node != null; node = node.Previous) {
                array[node.Depth] = node.Value;
            }

            return array;
        }
    }
}