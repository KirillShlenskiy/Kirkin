using System;
using System.Collections;
using System.Collections.Generic;

namespace Kirkin.Collections.Specialised
{
    /// <summary>
    /// High-performance collection which contains a single element.
    /// </summary>
    internal struct OneElementReadOnlyList<T> : IReadOnlyList<T>
    {
        private readonly T LoneItem;

        /// <summary>
        /// Always returns 1.
        /// </summary>
        public int Count => 1;

        /// <summary>
        /// Returns the item at index 0. Throws for any other index.
        /// </summary>
        public T this[int index]
        {
            get
            {
                if (index == 0) {
                    return LoneItem;
                }

                throw new IndexOutOfRangeException();
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="OneElementReadOnlyList{T}"/> which wraps the given item.
        /// </summary>
        public OneElementReadOnlyList(T item)
        {
            LoneItem = item;
        }

        /// <summary>
        /// Returns the struct enumerator over this instance.
        /// </summary>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// Returns the class enumerator over this instance.
        /// </summary>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new EnumeratorObject(this);
        }

        /// <summary>
        /// Returns the class enumerator over this instance.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new EnumeratorObject(this);
        }

        /// <summary>
        /// Struct enumerator.
        /// </summary>
        public struct Enumerator
        {
            private readonly OneElementReadOnlyList<T> Collection;
            private bool MovedNext;

            public T Current
            {
                get
                {
                    if (!MovedNext) {
                        throw new InvalidOperationException();
                    }

                    return Collection.LoneItem;
                }
            }

            internal Enumerator(OneElementReadOnlyList<T> collection)
            {
                Collection = collection;
                MovedNext = false;
            }

            public bool MoveNext()
            {
                if (MovedNext) {
                    return false;
                }

                MovedNext = true;

                return true;
            }
        }

        /// <summary>
        /// Reference type (slow) enumerator.
        /// </summary>
        sealed class EnumeratorObject : IEnumerator<T>
        {
            private readonly OneElementReadOnlyList<T> Collection;
            private bool MovedNext;

            public T Current
            {
                get
                {
                    if (!MovedNext) {
                        throw new InvalidOperationException();
                    }

                    return Collection.LoneItem;
                }
            }

            object IEnumerator.Current => Current;

            internal EnumeratorObject(OneElementReadOnlyList<T> collection)
            {
                Collection = collection;
                MovedNext = false;
            }

            public bool MoveNext()
            {
                if (MovedNext) {
                    return false;
                }

                MovedNext = true;

                return true;
            }

            public void Reset()
            {
                MovedNext = false;
            }

            public void Dispose()
            {
            }
        }
    }
}