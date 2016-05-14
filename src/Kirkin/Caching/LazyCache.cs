using System;
using System.Threading;

namespace Kirkin.Caching
{
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
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

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
}