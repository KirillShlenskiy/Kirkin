using System;
using System.Threading;

namespace Kirkin.Caching
{
    /// <summary>
    /// <see cref="ICache{T}" /> factory methods.
    /// </summary>
    public static class Cache
    {
        #region Factory methods

        /// <summary>
        /// Creates an immutable cache whose value
        /// is constant and immediately available.
        /// This cache cannot be invalidated.
        /// </summary>
        public static ICache<T> Constant<T>(T value)
        {
            return new ConstantCache<T>(value);
        }

        /// <summary>
        /// Creates a delegate-based cache which is thread-safe on execution and publication.
        /// </summary>
        public static ICache<T> Create<T>(Func<T> valueFactory)
        {
            return new LazyCache<T>(valueFactory);
        }

        /// <summary>
        /// Creates a delegate-based cache which is thread-safe on execution and publication.
        /// </summary>
        public static ICache<T> Create<TArg, T>(TArg arg, Func<TArg, T> valueFactory)
        {
            Projector<TArg, T> projector = new Projector<TArg, T>(arg, valueFactory);

            return new LazyCache<T>(projector.Execute);
        }

        sealed class Projector<TArg, TResult>
        {
            private readonly TArg Arg;
            private readonly Func<TArg, TResult> Projection;

            internal Projector(TArg arg, Func<TArg, TResult> projection)
            {
                if (projection == null) throw new ArgumentNullException("projection");

                Arg = arg;
                Projection = projection;
            }

            public TResult Execute()
            {
                return Projection(Arg);
            }
        }

        /// <summary>
        /// Thread-safe cache which uses volatile state to produce its value.
        /// The value returned by its <see cref="ICache{T}.Value"/> property
        /// is guaranteed to be correct for the current key value.
        /// </summary>
        internal static ICache<TValue> Create<TKey, TValue>(Func<TKey> volatileKeySelector, Func<TKey, TValue> valueFactory)
        {
            if (volatileKeySelector == null) throw new ArgumentNullException(nameof(volatileKeySelector));
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            return new VolatileCache<TKey, TValue>(volatileKeySelector, valueFactory);
        }

        /// <summary>
        /// Returns an <see cref="ICache{T}" /> instance which does
        /// not perform any actual caching. Its <see cref="ICache{T}.IsValid" />
        /// property will always have a value of false, and the value factory
        /// delegate will be invoked on every <see cref="ICache{T}.Value" /> access.
        /// </summary>
        public static ICache<T> Uncached<T>(Func<T> valueFactory)
        {
            if (valueFactory == null) throw new ArgumentNullException("valueFactory");

            return new UncachedImpl<T>(valueFactory);
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Simple, immutable, thread-safe cache which is always valid.
        /// </summary>
        sealed class ConstantCache<T> : ICache<T>
        {
            /// <summary>
            /// Value specified when this instance was created.
            /// </summary>
            public T Value { get; }

            /// <summary>
            /// Always returns true.
            /// </summary>
            public bool IsValid
            {
                get
                {
                    return true;
                }
            }

            /// <summary>
            /// Creates a new instance of <see cref="ConstantCache{T}" />.
            /// </summary>
            internal ConstantCache(T value)
            {
                Value = value;
            }

            /// <summary>
            /// Does nothing.
            /// </summary>
            public void Invalidate()
            {
                // Not throwing just in case we feed this instance to WithExpiry(TimeSpan), for example -
                // which will cause auto-invalidation. We should still work in those scenarios.
            }
        }

        /// <summary>
        /// Provides fast, lazy, thread-safe access to cached data.
        /// </summary>
        internal sealed class LazyCache<T>
            : ICache<T>
        {
            /// <summary>
            /// Factory method used to fully
            /// regenerate the cache when required.
            /// </summary>
            private readonly Func<T> ValueFactory;

            /// <summary>
            /// Latest lazy created by Invalidate().
            /// From the moment the instance is fully
            /// constructed this field can never be null.
            /// </summary>
            private Lazy<T> _lazy;

            /// <summary>
            /// Returns the cached value initialising it
            /// using the factory delegate if necessary.
            /// Guaranteed to return the latest value
            /// even if a call to Invalidate() is made
            /// while the value is being generated.
            /// </summary>
            public T Value
            {
                get
                {
                    Lazy<T> lazy = _lazy;
                    return lazy.IsValueCreated ? lazy.Value : GetValueSlow(lazy);
                }
            }

            /// <summary>
            /// Returns true if the cached value is current and ready to use.
            /// </summary>
            public bool IsValid
            {
                get { return _lazy.IsValueCreated; }
            }

            /// <summary>
            /// Creates a new instance of the class.
            /// </summary>
            internal LazyCache(Func<T> valueFactory)
            {
                if (valueFactory == null) throw new ArgumentNullException("valueFactory");

                ValueFactory = valueFactory;

                Invalidate();
            }

            /// <summary>
            /// Gets the value ensuring that it is still current at  the end of the operation.
            /// </summary>
            private T GetValueSlow(Lazy<T> currentLazy)
            {
                // Slow path: need to initialise value.
                T value;
                Lazy<T> startLazy;

                do
                {
                    // Keep ref to _lazy as it was
                    // at the start of the operation.
                    startLazy = currentLazy;

                    // Generate new, or use cached value.
                    value = startLazy.Value;

                    // If the Lazy<T> reference has been swapped,
                    // a call to Invalidate() must have happened
                    // while the value was being generated.
                    // In that case, let's start again.
                    currentLazy = Volatile.Read(ref _lazy);
                }
                while (startLazy != currentLazy);

                return value;
            }

            /// <summary>
            /// Invalidates the cache causing it to
            /// be rebuilt next time it is accessed.
            /// </summary>
            public void Invalidate()
            {
                Interlocked.Exchange(ref _lazy, new Lazy<T>(ValueFactory));
            }
        }

        sealed class UncachedImpl<T>
            : ICache<T>
        {
            private readonly Func<T> ValueFactory;

            public bool IsValid
            {
                get
                {
                    return false;
                }
            }

            public T Value
            {
                get
                {
                    return ValueFactory();
                }
            }

            public UncachedImpl(Func<T> valueFactory)
            {
                ValueFactory = valueFactory;
            }

            public void Invalidate()
            {
                // Not throwing just in case we feed this instance to WithExpiry(TimeSpan), for example -
                // which will cause auto-invalidation. We should still work in those scenarios.
            }
        }

        sealed class VolatileCache<TKey, TValue>
            : ICache<TValue>
        {
            readonly Func<TKey> VolatileKeySelector;
            readonly Func<TKey, TValue> ValueFactory;
            readonly SingleKeyValueCache<TKey, TValue> Cache;

            public bool IsValid
            {
                get
                {
                    return Cache.IsValid(VolatileKeySelector());
                }
            }

            public TValue Value
            {
                get
                {
                    TKey beforeKey = VolatileKeySelector();

                    while (true)
                    {
                        Thread.MemoryBarrier();
                        TValue value = Cache.GetValue(beforeKey);
                        Thread.MemoryBarrier();
                        TKey afterKey = VolatileKeySelector();

                        if (Equals(beforeKey, afterKey)) {
                            return value;
                        }

                        beforeKey = afterKey;
                    }
                }
            }

            internal VolatileCache(Func<TKey> volatileKeySelector, Func<TKey, TValue> valueFactory)
            {
                VolatileKeySelector = volatileKeySelector;
                ValueFactory = valueFactory;
                Cache = new SingleKeyValueCache<TKey, TValue>(valueFactory);
            }

            public void Invalidate()
            {
                Cache.Invalidate(VolatileKeySelector());
            }
        }

        #endregion
    }

    /// <summary>
    /// Contract for caches with key/value support.
    /// </summary>
    internal interface IKeyValueCache<TKey, TValue>
    {
        /// <summary>
        /// Returns the cached value appropriate for the given key initialising it if necessary.
        /// </summary>
        TValue GetValue(TKey key);

        /// <summary>
        /// Returns true if the cache contains a valid value for the given key.
        /// </summary>
        bool IsValid(TKey key);

        /// <summary>
        /// Invalidates the cache causing it to be rebuilt.
        /// </summary>
        void Invalidate(TKey key);
    }

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