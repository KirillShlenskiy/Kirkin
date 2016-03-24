using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Kirkin.Collections.Generic;

namespace Kirkin.Linq
{
    internal static class ExperimentalEnumerableExtensions
    {
        /// <summary>
        /// Enumerates the nested collections to produce a flattened
        /// unordered sequence using the specified selector delegate.
        /// </summary>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> collection, Func<T, IEnumerable<T>> nextLevelSelector)
            where T : class
        {
            if (collection == null) throw new ArgumentNullException("collection");

            EnumerableUtil.EnsureMaterialised(ref collection);

            HashSet<T> result = new HashSet<T>(ObjectReferenceEqualityComparer<T>.Default);

            FlattenImpl(collection, nextLevelSelector, result);

            return result;
        }

        private static void FlattenImpl<T>(IEnumerable<T> collection, Func<T, IEnumerable<T>> nextLevelSelector, HashSet<T> result)
        {
            foreach (T item in collection)
            {
                // Cicrular reference detection.
                if (result.Add(item))
                {
                    IEnumerable<T> nextLevel = nextLevelSelector(item);

                    if (nextLevel != null) {
                        FlattenImpl(nextLevel, nextLevelSelector, result);
                    }
                }
            }
        }

        sealed class ObjectReferenceEqualityComparer<T> : IEqualityComparer<T>
            where T : class
        {
            public static readonly ObjectReferenceEqualityComparer<T> Default = new ObjectReferenceEqualityComparer<T>();

            public bool Equals(T x, T y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(T obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}