using System;
using System.Threading;

namespace Kirkin.Caching
{
    /// <summary>
    /// Provides fast, lazy, thread-safe access
    /// to cached data with optional auto-expiry.
    /// </summary>
    public sealed class AutoExpireCache<T> : CacheBase<T>
    {
        private static readonly TimeSpan InfiniteTimeSpan = new TimeSpan(0, 0, 0, 0, Timeout.Infinite);
        private readonly Func<T> ValueFactory;
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
        internal AutoExpireCache(Func<T> valueFactory, TimeSpan expireAfter)
        {
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            if (expireAfter <= TimeSpan.Zero && expireAfter != InfiniteTimeSpan) {
                throw new ArgumentException("expireAfter");
            }

            ValueFactory = valueFactory;
            ExpireAfter = expireAfter;
        }

        /// <summary>
        /// Creates and returns the cached value.
        /// </summary>
        protected override T CreateValue()
        {
            return ValueFactory();
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
}