using System;
using System.Collections;
using System.Collections.Generic;

namespace Kirkin.Collections.Generic
{
    /// <summary>
    /// Simple read-only facade which provides access to items
    /// in the slice of an array whose bounds are defined by
    /// this instance's start index and count.
    /// </summary>
    internal readonly struct ArraySlice<T>
#if NET_40
        : IEnumerable<T>
#else
        : IReadOnlyList<T>
#endif
    {
        private readonly T[] _array;
        private readonly int _start;
        private readonly int _length;
        
        public int Length
        {
            get
            {
                return _length;
            }
        }

        public int Start
        {
            get
            {
                return _start;
            }
        }

        public bool IsDefault
        {
            get
            {
                return _array == null;
            }
        }

        public T this[int index]
        {
            get
            {
                return _array[_start + index];
            }
        }

        public ArraySlice(T[] array)
            : this(array, 0, array.Length)
        {
        }

        public ArraySlice(T[] array, int start)
            : this(array, start, array.Length - start)
        {
        }

        public ArraySlice(T[] array, int start, int length)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start));
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            if (start + length > array.Length) throw new ArgumentOutOfRangeException();

            _array = array;
            _start = start;
            _length = length;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_array, _start, Length);
        }

        #region IEnumerable<T> implementation

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            for (int i = _start; i < _start + _length; i++) {
                yield return _array[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = _start; i < _start + Length; i++) {
                yield return _array[i];
            }
        }

        #endregion

        public bool Contains(T item)
        {
            for (int i = _start; i < _start + Length; i++)
            {
                if (Equals(_array[i], item)) {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = _start; i < _start + Length; i++) {
                array[arrayIndex++] = _array[i];
            }
        }

        public ArraySlice<T> Slice(int start)
        {
            return new ArraySlice<T>(_array, _start + start, Length - start);
        }

        public ArraySlice<T> Slice(int start, int length)
        {
            return new ArraySlice<T>(_array, _start + start, length);
        }

#if !NET_40
        int IReadOnlyCollection<T>.Count
        {
            get
            {
                return Length;
            }
        }
#endif
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
            internal Enumerator(T[] array, int start, int length)
            {
                _array = array;
                _upperBoundExclusive = start + length;
                _index = start - 1;
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