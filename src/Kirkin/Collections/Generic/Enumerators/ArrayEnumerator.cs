﻿// This is *not* original work by Kirill Shlenskiy.
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

namespace Kirkin.Collections.Generic.Enumerators
{
    /// <summary>
    /// An array enumerator.
    /// </summary>
    /// <remarks>
    /// It is important that this enumerator does NOT implement <see cref="System.IDisposable"/>.
    /// We want the iterator to inline when we do foreach and to not result in
    /// a try/finally frame in the client.
    /// </remarks>
    public struct ArrayEnumerator<T>
    {
        /// <summary> 
        /// The array being enumerated.
        /// </summary>
        private readonly T[] _array;

        /// <summary>
        /// The currently enumerated position.
        /// </summary>
        /// <value>
        /// -1 before the first call to <see cref="MoveNext"/>.
        /// >= this.array.Length after <see cref="MoveNext"/> returns false.
        /// </value>
        private int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayEnumerator{T}"/> struct.
        /// </summary>
        /// <param name="array">The array to enumerate.</param>
        public ArrayEnumerator(T[] array)
        {
            _array = array;
            _index = -1;
        }

        /// <summary>
        /// Gets the currently enumerated value.
        /// </summary>
        public T Current
        {
            get
            {
                // PERF: no need to do a range check, we already did in MoveNext.
                // if user did not call MoveNext or ignored its result (incorrect use)
                // he will still get an exception from the array access range check.
                return _array[_index];
            }
        }

        /// <summary>
        /// Advances to the next value to be enumerated.
        /// </summary>
        /// <returns><c>true</c> if another item exists in the array; <c>false</c> otherwise.</returns>
        public bool MoveNext()
        {
            return ++_index < _array.Length;
        }
    }
}