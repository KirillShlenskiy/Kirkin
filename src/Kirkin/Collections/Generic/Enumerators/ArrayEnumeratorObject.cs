// This is *not* original work by Kirill Shlenskiy.
// It is taken in its entirety or derived from 
// CoreFX (https://github.com/dotnet/corefx).
// Original license below:

// The MIT License (MIT)

// Copyright (c) .NET Foundation and Contributors

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Kirkin.Collections.Generic.Enumerators
{
    /// <summary>
    /// <see cref="ArrayEnumeratorObject"/> factory methods.
    /// </summary>
    public static class ArrayEnumeratorObject
    {
        /// <summary>
        /// Creates an enumerator for the specified array.
        /// </summary>
        public static IEnumerator<T> Create<T>(T[] array)
        {
            return array.Length == 0
                ? ArrayEnumeratorObject<T>.EmptyEnumerator
                : new ArrayEnumeratorObject<T>(array);
        }
    }

    /// <summary>
    /// An array enumerator that implements <see cref="IEnumerator{T}"/> pattern (including <see cref="IDisposable"/>).
    /// </summary>
    public sealed class ArrayEnumeratorObject<T> : IEnumerator<T>
    {
        /// <summary>
        /// A shareable singleton for enumerating empty arrays.
        /// </summary>
        internal static readonly IEnumerator<T> EmptyEnumerator =
            new ArrayEnumeratorObject<T>(Array<T>.Empty);

        /// <summary>
        /// The array being enumerated.
        /// </summary>
        private readonly T[] _array;

        /// <summary>
        /// The currently enumerated position.
        /// </summary>
        /// <value>
        /// -1 before the first call to <see cref="MoveNext"/>.
        /// this.array.Length - 1 after MoveNext returns false.
        /// </value>
        private int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayEnumeratorObject{T}"/> class.
        /// </summary>
        internal ArrayEnumeratorObject(T[] array)
        {
            _index = -1;
            _array = array;
        }

        /// <summary>
        /// Gets the currently enumerated value.
        /// </summary>
        public T Current
        {
            get
            {
                // this.index >= 0 && this.index < this.array.Length
                // unsigned compare performs the range check above in one compare
                if ((uint)_index < (uint)_array.Length) {
                    return _array[_index];
                }

                // Before first or after last MoveNext.
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Gets the currently enumerated value.
        /// </summary>
        object IEnumerator.Current
        {
            get { return Current; }
        }

        /// <summary>
        /// If another item exists in the array, advances to the next value to be enumerated.
        /// </summary>
        /// <returns><c>true</c> if another item exists in the array; <c>false</c> otherwise.</returns>
        public bool MoveNext()
        {
            int newIndex = _index + 1;
            int length = _array.Length;

            // unsigned math is used to prevent false positive if index + 1 overflows.
            if ((uint)newIndex <= (uint)length)
            {
                _index = newIndex;
                return (uint)newIndex < (uint)length;
            }

            return false;
        }

        /// <summary>
        /// Resets enumeration to the start of the array.
        /// </summary>
        void IEnumerator.Reset()
        {
            _index = -1;
        }

        /// <summary>
        /// Disposes this enumerator.
        /// </summary>
        /// <remarks>
        /// Currently has no action.
        /// </remarks>
        public void Dispose()
        {
            // we do not have any native or disposable resources.
            // nothing to do here.
        }
    }
}