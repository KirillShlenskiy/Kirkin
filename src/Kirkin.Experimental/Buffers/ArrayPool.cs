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

using System.Threading;

namespace System.Buffers
{
    /// <summary>
    /// This class provides resource pooling for arrays of any type. This is can increase performance
    /// of certain applications where lots of arrays are created and destroyed in rapid succession.
    /// This class is thread-safe.
    /// </summary>
    public abstract class ArrayPool<T>
    {
        /// <summary>The default number of arrays that are available for rent.</summary>
        private const int DefaultNumberOfArraysPerBucket = 50;
        /// <summary>The default length of each Rent'able array; equal to 1MB in bytes</summary>
        private const int DefaultArrayLength = 1048576;

        private static ArrayPool<T> s_sharedInstance = null;

        /// <summary>
        /// Retrieves a shared instance pool that is thread-safe and can be used by 
        /// multiple components of the same system. When using the default Shared pool,
        /// buffers will be allocated when all of the pooled buffers have been exhausted.
        /// </summary>
        public static ArrayPool<T> Shared
        {
            get
            {
                ArrayPool<T> instance = Volatile.Read(ref s_sharedInstance);
                if (instance == null)
                {
                    Interlocked.CompareExchange(ref s_sharedInstance, new DefaultArrayPool<T>(DefaultArrayLength, DefaultNumberOfArraysPerBucket), null);
                    instance = s_sharedInstance;
                }

                return instance;
            }
        }

        /// <summary>
        /// Creates a new ArrayPool instance of the given type using the default configuration options.
        /// </summary>
        /// <returns>Returns a new ArrayPool<T> instance with the specified configuration options</return>
        public static ArrayPool<T> Create()
        {
            return Create(DefaultArrayLength, DefaultNumberOfArraysPerBucket);
        }

        /// <summary>
        /// Creates a new ArrayPool instance of the given type.
        /// </summary>
        /// <param name="maxArrayLength">The maximum length of any Rent request.</param>
        /// <param name-"numberOfArrays">The maximum number of arrays that will be rented.</param>
        /// <returns>Returns a new ArrayPool<T> instance with the specified configuration options</return>
        public static ArrayPool<T> Create(int maxArrayLength, int numberOfArrays)
        {
            return new DefaultArrayPool<T>(maxArrayLength, numberOfArrays);
        }

        /// <summary>
        /// Retrieves a buffer from the pool that is at least the requested length. This buffer is loaned
        /// to the caller and the caller must call Return when finished with the buffer.
        /// </summary>
        /// <param name="length">The number of elements in the desired buffer</param>
        /// <returns>Returns an array of type T that is at least 'length' in size</returns>
        public abstract T[] Rent(int minimumLength);

        /// <summary>
        /// Returns a buffer to the pool that has been Rented or Enlarged. Once a buffer has been
        /// Returned, the caller gives up all ownership of the buffer and cannot use the reference
        /// again. A buffer must only be Returned once.
        /// </summary>
        /// <param name="buffer">The buffer that will be Returned to the pool for general use.</param>
        /// <param name="clearArray">
        /// If true, the buffer will be cleared of any data; otherwise, the buffer will be returned with any
        /// data intact.
        /// </param>
        public abstract void Return(T[] buffer, bool clearArray = false);
    }
}