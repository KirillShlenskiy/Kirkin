using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Kirkin.Collections.Generic.Enumerators;

namespace Kirkin.Collections.Generic
{
    /// <summary>
    /// Generic collection which maintains an index of its items for fast lookup.
    /// </summary>
    public sealed class KeyedCollection<TKey, TItem>
        : KeyedCollectionBase<TKey, TItem>
    {
        /// <summary>
        /// Backing field for Values.
        /// </summary>
        private TItem[] _values;

        /// <summary>
        /// Cache of values held in the Items dictionary.
        /// </summary>
        protected sealed override TItem[] Values
        {
            get
            {
                if (_values == null)
                {
                    // PEFR: ToArray is optimized for ICollection,
                    // producing much better performance than ArrayBuilder.
                    _values = Items.Values.ToArray();
                }

                return _values;
            }
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public KeyedCollection(Func<TItem, TKey> keySelector)
            : base(new Dictionary<TKey, TItem>(), keySelector)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name='items'>
        /// Items.
        /// </param>
        /// <param name="keySelector">
        /// Delegate which extracts the key value from an item.
        /// </param>
        /// <exception cref='ArgumentNullException'>
        /// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
        /// </exception>
        public KeyedCollection(IEnumerable<TItem> items, Func<TItem, TKey> keySelector)
            : base(items.ToDictionary(keySelector), keySelector)
        {
        }

        /// <summary>
        /// Invalidated any cached information derived from the Items collection.
        /// </summary>
        protected sealed override void InvalidateValues()
        {
            _values = null;
        }
    }

    /// <summary>
    /// Base type for generic collections which maintain an index of their items for fast lookup.
    /// </summary>
    public abstract class KeyedCollectionBase<TKey, TItem>
        : ICollection<TItem>
    {
        /// <summary>
        /// Underlying dictionary of items.
        /// </summary>
        protected internal readonly IDictionary<TKey, TItem> Items; // Internal for testability.

        /// <summary>
        /// Gets the number of items in this collection.
        /// </summary>
        public int Count
        {
            get
            {
                return Items.Count;
            }
        }

        /// <summary>
        /// Gets a collection of keys present in the index.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get
            {
                return Items.Keys;
            }
        }

        /// <summary>
        /// Delegate used to extract unique keys from collection elements.
        /// </summary>
        public Func<TItem, TKey> KeySelector { get; }

        /// <summary>
        /// Cache of values held in the Items dictionary.
        /// </summary>
        protected abstract TItem[] Values { get; }

        /// <summary>
        /// Always returns false.
        /// </summary>
        protected bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Always returns false.
        /// </summary>
        bool ICollection<TItem>.IsReadOnly
        {
            get
            {
                return IsReadOnly;
            }
        }

        /// <summary>
        /// Gets an item with the given key.
        /// Throws if the key cannot be found in the index.
        /// </summary>
        public TItem this[TKey key]
        {
            get
            {
                if (TryGetItem(key, out TItem item)) {
                    return item;
                }

                throw new KeyNotFoundException($"The key {key} cannot be found in the index.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        protected KeyedCollectionBase(IDictionary<TKey, TItem> items, Func<TItem, TKey> keySelector)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (keySelector == null) throw new ArgumentNullException("keySelector");

            Items = items;
            KeySelector = keySelector;
        }

        #region Collection modification

        /// <summary>
        /// Insert the specified item.
        /// </summary>
        public void Add(TItem item)
        {
            Items.Add(KeySelector(item), item);
            InvalidateValues();
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            Items.Clear();
            InvalidateValues();
        }

        /// <summary>
        /// Delete the specified item.
        /// </summary>
        public bool Remove(TItem item)
        {
            if (Items.Remove(new KeyValuePair<TKey, TItem>(KeySelector(item), item)))
            {
                InvalidateValues();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Invalidated any cached information derived from the Items collection.
        /// </summary>
        protected abstract void InvalidateValues();

        #endregion

        #region Pure methods

        /// <summary>
        /// Copies all elements in this collection to the
        /// specified array starting at the given index.
        /// </summary>
        public void CopyTo(TItem[] array, int arrayIndex)
        {
            Values.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Determines whether the collection contains the specified value.
        /// </summary>
        public bool Contains(TItem item)
        {
            return Items.Contains(new KeyValuePair<TKey, TItem>(KeySelector(item), item));
        }

        #endregion

        #region Keyed access

        /// <summary>
        /// Returns true if the given key can be found in the index.
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            return Items.ContainsKey(key);
        }

        /// <summary>
        /// Returns the element with the specified
        /// key, or the default value for type.
        /// </summary>
        public bool TryGetItem(TKey key, out TItem item)
        {
            return Items.TryGetValue(key, out item);
        }

        #endregion

        #region IEnumerable implementation

        /// <summary>
        /// Gets the generic enumerator.
        /// </summary>
        public ArrayEnumerator<TItem> GetEnumerator()
        {
            return new ArrayEnumerator<TItem>(Values);
        }

        /// <summary>
        /// Gets the generic enumerator.
        /// </summary>
        IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator()
        {
            return Values.AsEnumerable().GetEnumerator(); // PERF: Faster than yield.
        }

        /// <summary>
        /// Gets the non-generic weakly typed enumerator (explicit).
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Values.AsEnumerable().GetEnumerator(); // PERF: Faster than yield.
        }

        #endregion
    }
}