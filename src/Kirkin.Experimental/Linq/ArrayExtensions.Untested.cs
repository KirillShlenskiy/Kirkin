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

//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Kirkin.Linq
//{
//	/// <summary>
//    /// Extension methods to be moved to ArrayExtensions after QA.
//    /// </summary>
//    internal static class ArrayExtensions_Untested
//    {
//		/// <summary>
//        /// Applies an accumulator function over a sequence.
//        /// </summary>
//        public static T Aggregate<T>(this T[] array, Func<T, T, T> func)
//        {
//            if (func == null) throw new ArgumentNullException(nameof(func));

//            if (array.Length == 0) {
//                return default(T);
//            }

//            T result = array[0];

//            for (int i = 1, n = array.Length; i < n; i++) {
//                result = func(result, array[i]);
//            }

//            return result;
//        }

//        /// <summary>
//        /// Applies an accumulator function over a sequence.
//        /// </summary>
//        public static TAccumulate Aggregate<TAccumulate, T>(this T[] array, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func)
//        {
//            if (func == null) throw new ArgumentNullException(nameof(func));

//            TAccumulate result = seed;

//            foreach (T v in array) {
//                result = func(result, v);
//            }

//            return result;
//        }

//        /// <summary>
//        /// Applies an accumulator function over a sequence.
//        /// </summary>
//        public static TResult Aggregate<TAccumulate, TResult, T>(this T[] array, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
//        {
//            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

//            return resultSelector(Aggregate(array, seed, func));
//        }

//		/// <summary>
//        /// Returns the last element of a sequence.
//        /// </summary>
//        public static T Last<T>(this T[] array)
//        {
//            // In the event of an empty array, generate the same
//            // exception that the linq extension method would.
//            return array.Length > 0
//                ? array[array.Length - 1]
//                : Enumerable.Last(array);
//        }

//        /// <summary>
//        /// Returns the last element of a sequence that satisfies a specified condition.
//        /// </summary>
//        public static T Last<T>(this T[] array, Func<T, bool> predicate)
//        {
//            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

//            for (int i = array.Length - 1; i >= 0; i--)
//            {
//                T item = array[i];

//                if (predicate(item)) {
//                    return item;
//                }
//            }

//            // Throw the same exception that LINQ would.
//            return Enumerable.Empty<T>().Last();
//        }

//        /// <summary>
//        /// Returns the last element of a sequence, or a default value if the sequence contains no elements.
//        /// </summary>
//        public static T LastOrDefault<T>(this T[] array)
//        {
//            return (array.Length == 0) ? default(T) : array[array.Length - 1];
//        }

//        /// <summary>
//        /// Returns the last element of a sequence that satisfies a condition or a default value if no such element is found.
//        /// </summary>
//        public static T LastOrDefault<T>(this T[] array, Func<T, bool> predicate)
//        {
//            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

//            for (int i = array.Length - 1; i >= 0; i--)
//            {
//                if (predicate(array[i])) {
//                    return array[i];
//                }
//            }

//            return default(T);
//        }

//        ///// <summary>
//        ///// Creates a dictionary based on the contents of this array.
//        ///// </summary>
//        //public static Dictionary<TKey, T> ToDictionary<TKey, T>(this T[] array, Func<T, TKey> keySelector)
//        //{
//        //    return ToDictionary(array, keySelector, EqualityComparer<TKey>.Default);
//        //}

//        ///// <summary>
//        ///// Creates a dictionary based on the contents of this array.
//        ///// </summary>
//        //public static Dictionary<TKey, TElement> ToDictionary<TKey, TElement, T>(this T[] array, Func<T, TKey> keySelector, Func<T, TElement> elementSelector)
//        //{
//        //    return ToDictionary(array, keySelector, elementSelector, EqualityComparer<TKey>.Default);
//        //}

//        ///// <summary>
//        ///// Creates a dictionary based on the contents of this array.
//        ///// </summary>
//        //public static Dictionary<TKey, T> ToDictionary<TKey, T>(this T[] array, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer)
//        //{
//        //    if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

//        //    var result = new Dictionary<TKey, T>(comparer);
//        //    foreach (var v in array)
//        //    {
//        //        result.Add(keySelector(v), v);
//        //    }

//        //    return result;
//        //}

//        ///// <summary>
//        ///// Creates a dictionary based on the contents of this array.
//        ///// </summary>
//        //public static Dictionary<TKey, TElement> ToDictionary<TKey, TElement, T>(this T[] array, Func<T, TKey> keySelector, Func<T, TElement> elementSelector, IEqualityComparer<TKey> comparer)
//        //{
//        //    if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
//        //    if (elementSelector == null) throw new ArgumentNullException(nameof(elementSelector));

//        //    var result = new Dictionary<TKey, TElement>(array.Length, comparer);
//        //    foreach (var v in array)
//        //    {
//        //        result.Add(keySelector(v), elementSelector(v));
//        //    }

//        //    return result;
//        //}
//    }
//}