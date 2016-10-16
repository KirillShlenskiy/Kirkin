using System;
using System.Threading;

using Kirkin.Caching.Internal;

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
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            Projector<TArg, T> projector = new Projector<TArg, T>(arg, valueFactory);

            return new LazyCache<T>(projector.Execute);
        }

        /// <summary>
        /// Thread-safe cache which uses volatile state to produce its value.
        /// The value returned by its <see cref="ICache{T}.Value"/> property
        /// is guaranteed to be correct for the current key value.
        /// </summary>
        public static ICache<TValue> Create<TKey, TValue>(Func<TKey> volatileKeySelector, Func<TKey, TValue> valueFactory)
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
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            return new UncachedImpl<T>(valueFactory);
        }

        #endregion

        #region Implementation

        sealed class Projector<TArg, TResult>
        {
            private readonly TArg Arg;
            private readonly Func<TArg, TResult> Projection;

            internal Projector(TArg arg, Func<TArg, TResult> projection)
            {
                Arg = arg;
                Projection = projection;
            }

            public TResult Execute()
            {
                return Projection(Arg);
            }
        }

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
}