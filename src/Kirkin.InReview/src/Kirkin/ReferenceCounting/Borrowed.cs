using System;
using System.Threading;

namespace Kirkin.ReferenceCounting
{
    /// <summary>
    /// Counted reference to a lazy-initialised shared resource.
    /// </summary>
    public sealed class Borrowed<T> : IDisposable
        where T : class
    {
        private readonly SharedResourceManager<T> Manager;
        private Lazy<T> Cache;

        /// <summary>
        /// Gets the resource managed by this instance.
        /// </summary>
        public T Value
        {
            get
            {
                Lazy<T> cache = Cache;

                if (cache == null) {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return cache.Value;
            }
        }

        /// <summary>
        /// Creates a new lazy-initialised disposable instance.
        /// </summary>
        internal Borrowed(SharedResourceManager<T> manager, Lazy<T> cache)
        {
            Manager = manager;
            Cache = cache;
        }

        /// <summary>
        /// Decrements the ref count and disposes of the resource if it reaches zero.
        /// </summary>
        public void Dispose()
        {
            Lazy<T> cache = Interlocked.Exchange(ref Cache, null);

            if (cache != null) {
                Manager.Release(this);
            }
        }
    }
}