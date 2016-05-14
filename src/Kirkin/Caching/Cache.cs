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
        /// Common base class for types which cache values.
        /// </summary>
        internal abstract class CloneableCache<T> : ICache<T>
        {
            /// <summary>
            /// Returns the cached value initialising it if necessary.
            /// </summary>
            public abstract T Value { get; }

            /// <summary>
            /// Returns true if the cached value is current and ready to use.
            /// </summary>
            public abstract bool IsValid { get; }

            /// <summary>
            /// Invalidates the cache causing it to be rebuilt.
            /// </summary>
            public abstract void Invalidate();

            /// <summary>
            /// Creates a clone of this <see cref="CloneableCache{T}" />.
            /// </summary>
            protected internal abstract CloneableCache<T> Clone();

            /// <summary>
            /// Returns a clone of the given cache which automatically
            /// invalidates its value after the given amount of time.
            /// </summary>
            public ICache<T> WithExpiry(TimeSpan expireAfter)
            {
                return new AutoExpireCacheWrapper<T>(this, expireAfter);
            }
        }

        /// <summary>
        /// Base class for thread-safe ICache implementations.
        /// </summary>
        abstract class CacheBase<T> : CloneableCache<T>
        {
            private readonly object StateLock = new object(); // Fast.
            private readonly object ValueGenerationLock = new object(); // Slow.
            private int Version; // Incremented, never reset.

            /// <summary>
            /// Provides direct access to the current value stored by this instance (valid or otherwise).
            /// </summary>
            protected T CurrentValue { get; private set; }

            /// <summary>
            /// Returns the cached value initialising it using the factory delegate if necessary.
            /// Guaranteed to return the latest value even if a call to Invalidate is made
            /// while the value is being generated.
            /// </summary>
            public override T Value
            {
                get
                {
                    T value;
                    return TryGetValue(out value) ? value : GetValueSlow();
                }
            }

            /// <summary>
            /// Returns true if the cached value is current and ready to use.
            /// </summary>
            public override bool IsValid // Explicit to discourage use where thread safety is required.
            {
                get
                {
                    CheckReentrancy();

                    lock (StateLock) {
                        return IsCurrentValueValid();
                    }
                }
            }

            /// <summary>
            /// Takes out the slow lock and returns the value. Generates and stores
            /// the new value in the process if a valid value is not already available.
            /// </summary>
            private T GetValueSlow()
            {
                while (true)
                {
                    // This lock gives us LazyCache semantics.
                    // If multiple calls to Value arrive at the same time,
                    // only one gets to create and store the value.
                    bool lockTaken = false;

                    if (ValueGenerationLock != null) {
                        Monitor.Enter(ValueGenerationLock, ref lockTaken);
                    }

                    try
                    {
                        T value;

                        if (TryGetValue(out value)) {
                            return value;
                        }

                        int version;
                        lock (StateLock) version = Version;
                        value = CreateValue();

                        lock (StateLock)
                        {
                            if (version == Version)
                            {
                                // No Invalidate call occurred while the
                                // value was being generated. Safe to store.
                                StoreValue(value);

                                return value;
                            }
                        }

                        // An Invalidate call happened while we were generating the value.
                        // We will release and re-obtain the lock and try again.
                    }
                    finally
                    {
                        if (lockTaken) {
                            Monitor.Exit(ValueGenerationLock);
                        }
                    }
                }
            }

            /// <summary>
            /// Invalidates the cache causing it to be rebuilt next time it is accessed.
            /// </summary>
            public override void Invalidate()
            {
                CheckReentrancy();

                lock (StateLock)
                {
                    unchecked { Version++; }
                    OnInvalidate();
                }
            }

            /// <summary>
            /// Reads, validates and immediately returns the current value in a thread-safe
            /// manner. Returns false if the current value is not yet available, or is invalid.
            /// </summary>
            public bool TryGetValue(out T value)
            {
                CheckReentrancy();

                lock (StateLock)
                {
                    if (IsCurrentValueValid())
                    {
                        value = CurrentValue;
                        return true;
                    }

                    OnInvalidate();
                }

                value = default(T);
                return false;
            }

            /// <summary>
            /// Ensures that StateLock is not already taken.
            /// </summary>
            private void CheckReentrancy()
            {
#if !NET_40 && !__MOBILE__
                if (Monitor.IsEntered(StateLock)) {
                    throw new InvalidOperationException("StateLock already taken out. Re-entrancy prohibited.");
                }
#endif
            }

            /// <summary>
            /// When overridden in a derived class, creates and returns the cached value.
            /// Do *NOT* access shared (instance) state from within this method.
            /// </summary>
            protected abstract T CreateValue();

            /// <summary>
            /// When overridden in a derived class, performs a check to see if the value is still current.
            /// It is safe to access shared (instance) state from within this method.
            /// </summary>
            protected abstract bool IsCurrentValueValid();

            /// <summary>
            /// When overridden in a derived class, performs additional invalidation actions.
            /// It is safe to access shared (instance) state from within this method.
            /// </summary>
            protected abstract void OnInvalidate();

            /// <summary>
            /// Stores the newly created value.
            /// It is safe to access shared (instance) state from within this method.
            /// </summary>
            protected virtual void StoreValue(T newValue)
            {
                CurrentValue = newValue;
            }
        }

        /// <summary>
        /// Provides fast, lazy, thread-safe access
        /// to cached data with optional auto-expiry.
        /// </summary>
        sealed class AutoExpireCacheWrapper<T> : CacheBase<T>
        {
            private static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, Timeout.Infinite);
            private readonly CloneableCache<T> Inner;
            private int EnvironmentTicksAtLastStoreValue = -1; // Reset when invalidated.

            /// <summary>
            /// Gets the duration of the time interval after
            /// which the newly generated value becomes invalid
            /// (specified when this instance was created).
            /// </summary>
            public TimeSpan ExpireAfter { get; }

            /// <summary>
            /// Creates a new instance of the class with the given expiry parameter.
            /// </summary>
            internal AutoExpireCacheWrapper(CloneableCache<T> inner, TimeSpan expireAfter)
            {
                if (inner == null) throw new ArgumentNullException("inner");

                if (expireAfter <= TimeSpan.Zero && expireAfter != InfiniteTimeSpan) {
                    throw new ArgumentException("expireAfter");
                }

                Inner = inner.Clone();
                ExpireAfter = expireAfter;
            }

            /// <summary>
            /// Creates a clone of this cache.
            /// </summary>
            protected internal override CloneableCache<T> Clone()
            {
                return new AutoExpireCacheWrapper<T>(Inner, ExpireAfter);
            }

            /// <summary>
            /// Creates and returns the cached value.
            /// </summary>
            protected override T CreateValue()
            {
                return Inner.Value;
            }

            /// <summary>
            /// Performs a check to see if the value is still current.
            /// </summary>
            protected override bool IsCurrentValueValid()
            {
                return EnvironmentTicksAtLastStoreValue != -1 &&
                    (ExpireAfter == InfiniteTimeSpan ||
                     ExpireAfter.TotalMilliseconds > (Environment.TickCount - EnvironmentTicksAtLastStoreValue));
            }

            /// <summary>
            /// Performs additional invalidation actions.
            /// </summary>
            protected override void OnInvalidate()
            {
                Inner.Invalidate();
                EnvironmentTicksAtLastStoreValue = -1;
            }

            /// <summary>
            /// Stores the value after it's been created, along
            /// with the additional state required for IsValid.
            /// </summary>
            protected override void StoreValue(T newValue)
            {
                base.StoreValue(newValue);

                EnvironmentTicksAtLastStoreValue = Environment.TickCount;
            }
        }

        /// <summary>
        /// Simple, immutable, thread-safe cache which is always valid.
        /// </summary>
        sealed class ConstantCache<T> : CloneableCache<T>
        {
            /// <summary>
            /// Value specified when this instance was created.
            /// </summary>
            public override T Value { get; }

            /// <summary>
            /// Always returns true.
            /// </summary>
            public override bool IsValid
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
            public override void Invalidate()
            {
                // Not throwing just in case we feed this instance to WithExpiry(TimeSpan), for example -
                // which will cause auto-invalidation. We should still work in those scenarios.
            }

            /// <summary>
            /// Creates a clone of this cache.
            /// </summary>
            protected internal override CloneableCache<T> Clone()
            {
                // This instance does not hold any mutable state. No point in creating a clone.
                return this;
            }
        }

        /// <summary>
        /// Provides fast, lazy, thread-safe access to cached data.
        /// </summary>
        internal sealed class LazyCache<T> : CloneableCache<T>
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
            public override T Value
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
            public override bool IsValid
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
            public override void Invalidate()
            {
                Interlocked.Exchange(ref _lazy, new Lazy<T>(ValueFactory));
            }

            /// <summary>
            /// Creates a clone of this cache.
            /// </summary>
            protected internal override CloneableCache<T> Clone()
            {
                return new LazyCache<T>(ValueFactory);
            }
        }

        sealed class UncachedImpl<T> : CloneableCache<T>
        {
            private readonly Func<T> ValueFactory;

            public override bool IsValid
            {
                get
                {
                    return false;
                }
            }

            public override T Value
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

            public override void Invalidate()
            {
                // Not throwing just in case we feed this instance to WithExpiry(TimeSpan), for example -
                // which will cause auto-invalidation. We should still work in those scenarios.
            }

            protected internal override CloneableCache<T> Clone()
            {
                return new UncachedImpl<T>(ValueFactory);
            }
        }

        sealed class VolatileCache<TKey, TValue> : CloneableCache<TValue>
        {
            readonly Func<TKey> VolatileKeySelector;
            readonly Func<TKey, TValue> ValueFactory;
            readonly SingleKeyValueCache<TKey, TValue> Cache;

            public override bool IsValid
            {
                get
                {
                    return Cache.IsValid(VolatileKeySelector());
                }
            }

            public override TValue Value
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

            public override void Invalidate()
            {
                Cache.Invalidate(VolatileKeySelector());
            }

            protected internal override CloneableCache<TValue> Clone()
            {
                return new VolatileCache<TKey, TValue>(VolatileKeySelector, ValueFactory);
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