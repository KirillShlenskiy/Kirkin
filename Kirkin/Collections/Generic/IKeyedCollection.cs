using System;
using System.Collections.Generic;

namespace Kirkin.Collections.Generic
{
    /// <summary>
    /// Operation contract for a common collection
    /// with elements identified by their unique keys.
    /// </summary>
    internal interface IKeyedCollection<TKey, TItem> : ICollection<TItem> // To be retired.
    {
        /// <summary>
        /// Delegate used to extract key values from items
        /// for the purpose of interacting with the index.
        /// </summary>
        Func<TItem, TKey> KeySelector { get; }

        /// <summary>
        /// Enumerates the keys present in this collection.
        /// </summary>
        ICollection<TKey> Keys { get; }

        /// <summary>
        /// Gets an item with the given key.
        /// Throws if the key cannot be found in the index.
        /// </summary>
        TItem this[TKey key] { get; }

        /// <summary>
        /// Returns true if the given key is present in this collection.
        /// </summary>
        bool ContainsKey(TKey key);

        /// <summary>
        /// Returns the element with the specified
        /// key, or the default value for type.
        /// </summary>
        bool TryGetItem(TKey key, out TItem item);
    }
}