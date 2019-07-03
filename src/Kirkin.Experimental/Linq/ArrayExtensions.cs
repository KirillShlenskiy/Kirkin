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

#if FAST_ARRAY_LINQ
namespace System.Linq
#else
using System;
using System.Linq;

namespace Kirkin.Linq
#endif
{
    /// <summary>
    /// LINQ extension method overrides that offer greater
    /// efficiency for arrays than the standard LINQ methods.
    /// </summary>
    public static class ArrayExtensions
    {
        // PERF: Array arg null checks largely skipped if subsequently
        // accessing Length, indexing into, or iterating through the array.
        // NullReferenceException will be thrown - inconsistent
        // with System.Linq, but consistent with ImmutableArray<T>.

        /// <summary>
        /// Gets a value indicating whether any elements are in this collection.
        /// </summary>
        public static bool Any<T>(this T[] array)
        {
            return array.Length != 0;
        }

        /// <summary>
        /// Gets a value indicating whether any elements are in this collection
        /// that match a given condition.
        /// </summary>
        public static bool Any<T>(this T[] array, Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            foreach (T item in array)
            {
                if (predicate(item)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a value indicating whether all elements in this collection
        /// match a given condition.
        /// </summary>
        public static bool All<T>(this T[] array, Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            foreach (T item in array)
            {
                if (!predicate(item)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the number of elements in the collection.
        /// </summary>
        public static int Count<T>(this T[] array)
        {
            return array.Length;
        }

        /// <summary>
        /// Returns the number of elements in the collection matching the given predicate.
        /// </summary>
        public static int Count<T>(this T[] array, Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            int count = 0;

            foreach (T item in array)
            {
                if (predicate(item))
                {
                    checked {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Returns the element at a specified index in a sequence.
        /// </summary>
        public static T ElementAt<T>(this T[] array, int index)
        {
            return array[index];
        }

        /// <summary>
        /// Returns the element at a specified index in a sequence or a default value if the index is out of range.
        /// </summary>
        public static T ElementAtOrDefault<T>(this T[] array, int index)
        {
            return (index < 0 || index >= array.Length)
                ? default(T)
                : array[index];
        }

        /// <summary>
        /// Returns the first element in a sequence that satisfies a specified condition.
        /// </summary>
        public static T First<T>(this T[] array, Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            foreach (T item in array)
            {
                if (predicate(item)) {
                    return item;
                }
            }

            return Enumerable.Empty<T>().First(); // LINQ exception.
        }

        /// <summary>
        /// Returns the first element in a sequence that satisfies a specified condition.
        /// </summary>
        public static T First<T>(this T[] array)
        {
            return (array.Length > 0)
                ? array[0]
                : Enumerable.First(array); // LINQ exception.
        }

        /// <summary>
        /// Returns the first element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        public static T FirstOrDefault<T>(this T[] array)
        {
            return (array.Length == 0) ? default(T) : array[0];
        }

        /// <summary>
        /// Returns the first element of the sequence that satisfies a condition or a default value if no such element is found.
        /// </summary>
        public static T FirstOrDefault<T>(this T[] array, Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            foreach (T item in array)
            {
                if (predicate(item)) {
                    return item;
                }
            }

            return default(T);
        }

        /// <summary>
        /// Returns the only element of a sequence, and throws an exception if there is not exactly one element in the sequence.
        /// </summary>
        public static T Single<T>(this T[] array)
        {
            return (array.Length == 1)
                ? array[0]
                : Enumerable.Single(array); // LINQ exception.
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.
        /// </summary>
        public static T Single<T>(this T[] array, Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            bool first = true;
            T result = default(T);

            foreach (T item in array)
            {
                if (predicate(item))
                {
                    if (!first) {
                        Enumerable.Single(array); // LINQ exception.
                    }

                    first = false;
                    result = item;
                }
            }

            if (first) {
                Enumerable.Empty<T>().Single(); // LINQ exception.
            }

            return result;
        }

        /// <summary>
        /// Returns the only element of a sequence, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.
        /// </summary>
        public static T SingleOrDefault<T>(this T[] array)
        {
            if (array.Length == 0) return default(T);
            if (array.Length == 1) return array[0];

            return Enumerable.SingleOrDefault(array);
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition or a default value if no such element exists; this method throws an exception if more than one element satisfies the condition.
        /// </summary>
        public static T SingleOrDefault<T>(this T[] array, Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            bool first = true;
            T result = default(T);

            foreach (T item in array)
            {
                if (predicate(item))
                {
                    if (!first) {
                        Enumerable.Single(array); // LINQ exception.
                    }

                    first = false;
                    result = item;
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a copy of this array.
        /// </summary>
        public static T[] ToArray<T>(this T[] array)
        {
            T[] clone = new T[array.Length];

            Array.Copy(array, 0, clone, 0, array.Length);

            return clone;
        }
    }
}