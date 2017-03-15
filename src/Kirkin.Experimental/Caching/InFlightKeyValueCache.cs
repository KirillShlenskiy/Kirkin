using System;
using System.Collections.Generic;

namespace Kirkin.Caching
{
    /// <summary>
    /// Key-value cache implementation which allows multiple GetValue calls
    /// arriving at the same time to share the in-flight ValueFactory invocation.
    /// </summary>
    public sealed class InFlightKeyValueCache<TKey, TValue>
    {
        private readonly Func<TKey, TValue> ValueFactory;
        private readonly Dictionary<TKey, Lazy<TValue>> Cache = new Dictionary<TKey, Lazy<TValue>>();

        /// <summary>
        /// Creates a new instance of <see cref="InFlightKeyValueCache{TKey, TValue}"/>.
        /// </summary>
        public InFlightKeyValueCache(Func<TKey, TValue> valueFactory)
        {
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            ValueFactory = valueFactory;
        }

        /// <summary>
        /// Invokes the value factory delegate to produce a value for the given
        /// key, or waits for an existing value factory delegate invocation to
        /// complete if one is already in progress, then returns its value.
        /// </summary>
        public TValue GetValue(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            Lazy<TValue> lazy;

            lock (Cache)
            {
                if (!Cache.TryGetValue(key, out lazy))
                {
                    lazy = new Lazy<TValue>(() => ValueFactory(key));

                    Cache.Add(key, lazy);
                }
            }

            TValue value;

            try
            {
                // Wait for the ValueFactory invocation to complete.
                value = lazy.Value;
            }
            finally
            {
                // At this point the ValueFactory invocation has completed and the value is ready.
                // We will invalidate the cache entry if another thread hasn't done so already.
                lock (Cache)
                {
                    if (Cache.TryGetValue(key, out Lazy<TValue> currentLazy) && lazy == currentLazy) {
                        Cache.Remove(key);
                    }
                }
            }

            return value;
        }
    }
}