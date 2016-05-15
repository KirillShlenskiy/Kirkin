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

namespace System.Buffers
{
    internal sealed class DefaultArrayPool<T> : ArrayPool<T>
    {
        private const int MinimiumArraySize = 16;
        private ArrayPoolBucket<T>[] _buckets;

        internal DefaultArrayPool(int maxLength, int arraysPerBucket)
        {
            if (maxLength <= 0)
                throw new ArgumentOutOfRangeException("maxLength");
            if (arraysPerBucket <= 0)
                throw new ArgumentOutOfRangeException("arraysPerBucket");

            // Our bucketing algorithm has a minimum length of 16
            if (maxLength < MinimiumArraySize)
                maxLength = MinimiumArraySize;
            
            int maxBuckets = Utilities.SelectBucketIndex(maxLength);
            _buckets = new ArrayPoolBucket<T>[maxBuckets + 1];
            for (int i = 0; i < _buckets.Length; i++)
                _buckets[i] = new ArrayPoolBucket<T>(Utilities.GetMaxSizeForBucket(i), arraysPerBucket);
        }

        public override T[] Rent(int minimumLength)
        {
            if (minimumLength <= 0)
                throw new ArgumentOutOfRangeException("minimumLength");

            int index = Utilities.SelectBucketIndex(minimumLength);
            if (index < _buckets.Length)
            {
                T[] buffer = null;

                // Search for an array starting at the 'index' bucket. If the bucket
                // is empty, bump up to the next higher bucket and try that one
                for (int i = index; i < _buckets.Length; i++)
                {
                    buffer = _buckets[i].Rent();

                    // If the bucket has an array left and returned it, give it to the caller
                    if (buffer != null)
                    {
                        return buffer;
                    }
                }
            }

            // Gettings here means we have too big of a request OR all the buckets from 
            // index through _buckets.Length are taken so we need to allocate a buffer on-demand.
            return new T[Utilities.GetMaxSizeForBucket(index)];
        }

        public override void Return(T[] buffer, bool clearArray = false)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            // If we can tell that the buffer was allocated, drop it. Otherwise, check if we have space in the pool
            int bucket = Utilities.SelectBucketIndex(buffer.Length);
            if (bucket < _buckets.Length)
            {
                // Clear the array if the user requests
                if (clearArray) Array.Clear(buffer, 0, buffer.Length);

                _buckets[bucket].Return(buffer);
            }
        }
    }
}