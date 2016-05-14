using System;
using System.Threading;

namespace Kirkin.Caching
{
    /// <summary>
    /// Thread-safe cache capable of holding a single key/value combination at any given time.
    /// </summary>
    internal sealed class SingleKeyValueCache<TKey, TValue>
        : IKeyValueCache<TKey, TValue>
    {
        // Immutable state.
        private readonly Func<TKey, TValue> ValueFactory;
        private readonly object CreateValueLock = new object();

        // Mutable state.
        private Box ValueBox;

        /// <summary>
        /// Creates a new instance of <see cref="SingleKeyValueCache{TKey, TValue}"/>
        /// with the given value-producting delegate.
        /// </summary>
        public SingleKeyValueCache(Func<TKey, TValue> valueFactory)
        {
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            ValueFactory = valueFactory;
        }

        /// <summary>
        /// Returns the cached value appropriate for the given key initialising it if necessary.
        /// </summary>
        public TValue GetValue(TKey key)
        {
            TValue value;
            return TryGetValue(key, out value) ? value : GetValueSlow(key);
        }

        /// <summary>
        /// Retrieves the value associated with the given
        /// key if it exists, immediately, without locking.
        /// </summary>
        private bool TryGetValue(TKey key, out TValue value)
        {
            // Non-volatile read. Worst case scenario is we won't see
            // the latest value, lock on CreateValueLock and get it then.
            Box box = ValueBox;

            if (box != null && Equals(key, box.Key))
            {
                value = box.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Retrieves the value associated with the given
        /// key. Creates and stores the value if necessary.
        /// </summary>
        private TValue GetValueSlow(TKey key)
        {
            TValue value;

            lock (CreateValueLock)
            {
                if (TryGetValue(key, out value)) {
                    return value;
                }

                // Create and store value.
                value = ValueFactory(key);

                ValueBox = new Box {
                    Key = key,
                    Value = value
                };
            }

            return value;
        }

        /// <summary>
        /// Returns true if the cache contains a valid value for the given key.
        /// </summary>
        public bool IsValid(TKey key)
        {
            Box box = Volatile.Read(ref ValueBox);

            return box != null
                && Equals(key, box.Key);
        }

        /// <summary>
        /// Invalidates the cache causing it to be rebuilt.
        /// </summary>
        public void Invalidate(TKey key)
        {
            Box box = Volatile.Read(ref ValueBox);

            if (box != null && Equals(box.Key, key)) {
                Interlocked.CompareExchange(ref ValueBox, null, box);
            }
        }

        sealed class Box
        {
            internal TKey Key;
            internal TValue Value;
        }
    }
}