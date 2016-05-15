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

using System.Runtime.CompilerServices;

namespace System.Buffers
{
    internal static class Utilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int SelectBucketIndex(int bufferSize)
        {
            uint bitsRemaining = ((uint)bufferSize - 1) >> 4;
            int poolIndex = 0;

            while (bitsRemaining > 0)
            {
                bitsRemaining >>= 1;
                poolIndex++;
            }

            return poolIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetMaxSizeForBucket(int binIndex)
        {
            checked
            {
                int result = 2;
                int shifts = binIndex + 3;
                result <<= shifts;
                return result;
            }
        }
    }
}