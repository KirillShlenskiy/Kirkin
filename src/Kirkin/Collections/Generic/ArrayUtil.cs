using System;

namespace Kirkin.Collections.Generic
{
    /// <summary>
    /// Common utilities for working with arrays.
    /// </summary>
    internal static class ArrayUtil
    {
        /// <summary>
        /// Reduces the length of the array to, at most, the given number of elements.
        /// </summary>
        internal static T[] Trim<T>(T[] array, int maxLength)
        {
            if (array.Length <= maxLength) {
                return array;
            }

            T[] copy = new T[maxLength];

            Array.Copy(array, 0, copy, 0, maxLength);

            return copy;
        }
    }
}