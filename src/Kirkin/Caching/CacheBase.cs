#if !NET_40 && !__MOBILE__
    #define PROHIBIT_REENTRANCY
#endif

using System;
using System.Threading;

namespace Kirkin.Caching
{
    /// <summary>
    /// Base class for thread-safe ICache implementations.
    /// </summary>
    public abstract class CacheBase<T> : ICache<T>
    {
        private readonly object StateLock = new object(); // Fast.
        private readonly object ValueGenerationLock = new object(); // Slow.
        private int Version; // Incremented, never reset.
        private T _currentValue;

        /// <summary>
        /// Provides direct access to the current value stored by this instance (valid or otherwise).
        /// </summary>
        protected T CurrentValue
        {
            get
            {
                return _currentValue;
            }
        }

        /// <summary>
        /// Returns the cached value initialising it using the factory delegate if necessary.
        /// Guaranteed to return the latest value even if a call to Invalidate is made
        /// while the value is being generated.
        /// </summary>
        public T Value
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
        public bool IsValid
        {
            get
            {
#if PROHIBIT_REENTRANCY
                CheckReentrancy();
#endif
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
        public void Invalidate()
        {
#if PROHIBIT_REENTRANCY
            CheckReentrancy();
#endif
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
#if PROHIBIT_REENTRANCY
            CheckReentrancy();
#endif
            lock (StateLock)
            {
                if (IsCurrentValueValid())
                {
                    value = _currentValue;
                    return true;
                }

                OnInvalidate();
            }

            value = default(T);
            return false;
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
            _currentValue = newValue;
        }

#if PROHIBIT_REENTRANCY
        /// <summary>
        /// Ensures that StateLock is not already taken.
        /// </summary>
        private void CheckReentrancy()
        {
            if (Monitor.IsEntered(StateLock)) {
                throw new InvalidOperationException("StateLock already taken out. Re-entrancy prohibited.");
            }
        }
#endif
    }
}