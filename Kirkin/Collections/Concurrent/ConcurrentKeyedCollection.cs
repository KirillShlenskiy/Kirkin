using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

using Kirkin.Caching;
using Kirkin.Collections.Generic;

namespace Kirkin.Collections.Concurrent
{
    /// <summary>
    /// Generic collection which maintains an index of its items for fast lookup.
    /// </summary>
    public sealed class ConcurrentKeyedCollection<TKey, TItem>
        : KeyedCollectionBase<TKey, TItem>
    {
        /// <summary>
        /// Backing field for Values.
        /// </summary>
        private readonly Cache.LazyCache<TItem[]> _values;

        /// <summary>
        /// Cache of values held in the Items dictionary.
        /// </summary>
        protected sealed override TItem[] Values
        {
            get
            {
                return _values.Value;
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ConcurrentKeyedCollection(Func<TItem, TKey> keySelector)
            : base(new ConcurrentDictionary<TKey, TItem>(), keySelector)
        {
            _values = new Cache.LazyCache<TItem[]>(() => Items.Values.ToArray());
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
        public ConcurrentKeyedCollection(IEnumerable<TItem> items, Func<TItem, TKey> keySelector)
            : this(keySelector)
        {
            if (items == null) throw new ArgumentNullException("items");

            foreach (TItem item in items) {
                Items.Add(keySelector(item), item);
            }
        }

        /// <summary>
        /// Invalidated any cached information derived from the Items collection.
        /// </summary>
        protected sealed override void InvalidateValues()
        {
            _values.Invalidate();
        }
    }
}