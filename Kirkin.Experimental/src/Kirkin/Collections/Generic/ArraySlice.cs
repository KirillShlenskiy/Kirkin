using System;
using System.Collections;
using System.Collections.Generic;

namespace Kirkin.Collections.Generic
{
    /// <summary>
    /// Simple read-only facade which provides access to items
    /// in the slice of an array whose bounds are defined by
    /// this instance's offset and count.
    /// </summary>
    public struct ArraySlice<T>
        : IReadOnlyList<T>, ICollection<T>
    {
        private readonly T[] _array;
        private readonly int _offset;
        
        public int Count { get; }

        public T this[int index]
        {
            get
            {
                return _array[_offset + index];
            }
        }

        public ArraySlice(T[] array, int offset, int count)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

            _array = array;
            _offset = offset;
            Count = count;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_array, _offset, Count);
        }

        #region IEnumerable<T> implementation

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            for (int i = _offset; i < _offset + Count; i++) {
                yield return _array[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = _offset; i < _offset + Count; i++) {
                yield return _array[i];
            }
        }

        #endregion

        #region ICollection<T> implementation

        int ICollection<T>.Count
        {
            get
            {
                return Count;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<T>.Contains(T item)
        {
            for (int i = _offset; i < _offset + Count; i++)
            {
                if (Equals(_array[i], item)) {
                    return true;
                }
            }

            return false;
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            for (int i = _offset; i < _offset + Count; i++) {
                array[arrayIndex++] = _array[i];
            }
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// An array slice enumerator.
        /// </summary>
        public struct Enumerator
        {
            /// <summary> 
            /// The array being enumerated.
            /// </summary>
            private readonly T[] _array;
            private readonly int _upperBoundExclusive;

            /// <summary>
            /// The currently enumerated position.
            /// </summary>
            /// <value>
            /// -1 before the first call to <see cref="MoveNext"/>.
            /// >= this.array.Length after <see cref="MoveNext"/> returns false.
            /// </value>
            private int _index;

            /// <summary>
            /// Initializes a new instance of the <see cref="Enumerator"/> struct.
            /// </summary>
            internal Enumerator(T[] array, int offset, int length)
            {
                _array = array;
                _upperBoundExclusive = offset + length;
                _index = offset - 1;
            }

            /// <summary>
            /// Gets the currently enumerated value.
            /// </summary>
            public T Current
            {
                get
                {
                    return _array[_index];
                }
            }

            /// <summary>
            /// Advances to the next value to be enumerated.
            /// </summary>
            public bool MoveNext()
            {
                return ++_index < _upperBoundExclusive;
            }
        }
    }
}