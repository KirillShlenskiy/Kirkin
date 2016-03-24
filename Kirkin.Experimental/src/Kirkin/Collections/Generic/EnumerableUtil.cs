using System;
using System.Collections.Generic;
using System.Linq;

namespace Kirkin.Collections.Generic
{
    /// <summary>
    /// Static sequence utility methods.
    /// </summary>
    public static class EnumerableUtil
    {
        /// <summary>
        /// Copies the sequence to a new materialised collection
        /// unless it is a known materialised collection type.
        /// </summary>
        public static void EnsureMaterialised<T>(ref IEnumerable<T> collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            if (collection is ICollection<T>) return;
#if NET_45
            if (collection is IReadOnlyCollection<T>) return;
#endif
            collection = collection.ToArray();
        }
    }
}