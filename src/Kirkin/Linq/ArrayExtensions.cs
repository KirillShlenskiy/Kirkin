﻿namespace System.Linq
{
    /// <summary>
    /// LINQ extension method overrides that offer greater efficiency for arrays than the standard LINQ methods.
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

            for (int i = 0; i < array.Length; i++)
            {
                if (predicate(array[i])) {
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

            foreach (T v in array)
            {
                if (!predicate(v)) {
                    return false;
                }
            }

            return true;
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

            foreach (T v in array)
            {
                if (predicate(v)) {
                    return v;
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

            foreach (T v in array)
            {
                if (predicate(v)) {
                    return v;
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

            foreach (T v in array)
            {
                if (predicate(v))
                {
                    if (!first) {
                        Enumerable.Single(array); // LINQ exception.
                    }

                    first = false;
                    result = v;
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

            foreach (T v in array)
            {
                if (predicate(v))
                {
                    if (!first) {
                        Enumerable.Single(array); // throw the same exception as LINQ would
                    }

                    first = false;
                    result = v;
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