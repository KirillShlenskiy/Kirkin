using System;
using System.Collections.Generic;

namespace Kirkin.ReferenceCounting
{
    /// <summary>
    /// Provides a mechanism for the lazy creation and deterministic disposal of shared resources.
    /// </summary>
    public sealed class SharedResourceManager<T>
        where T : class
    {
        private readonly Func<T> ResourceFactory;
        private readonly HashSet<Borrowed<T>> Consumers = new HashSet<Borrowed<T>>();
        private Lazy<T> Cache;

        /// <summary>
        /// Gets the value indicating that this instance is allowed
        /// to create new resources after the value is cleaned up.
        /// </summary>
        public bool AllowResurrect { get; }

        /// <summary>
        /// Gets the number of references to the shared resource.
        /// </summary>
        public int ReferenceCount
        {
            get
            {
                lock (Consumers) {
                    return Consumers.Count;
                }
            }
        }

        /// <summary>
        /// Creates a new reference counter instance.
        /// </summary>
        public SharedResourceManager(Func<T> resourceFactory, bool allowResurrect = false)
        {
            if (resourceFactory == null) throw new ArgumentNullException(nameof(resourceFactory));

            ResourceFactory = resourceFactory;
            AllowResurrect = allowResurrect;
            Cache = new Lazy<T>(resourceFactory);
        }

        /// <summary>
        /// Produces a counted reference to the lazily initialized shared resource.
        /// </summary>
        public Borrowed<T> Borrow()
        {
            lock (Consumers)
            {
                if (Cache == null)
                {
                    if (!AllowResurrect) {
                        throw new InvalidOperationException("Resource already disposed.");
                    }

                    // Resurrect.
                    Cache = new Lazy<T>(ResourceFactory);
                }

                Borrowed<T> borrowed = new Borrowed<T>(this, Cache);

                Consumers.Add(borrowed);

                return borrowed;
            }
        }

        internal void Release(Borrowed<T> borrowed)
        {
            Lazy<T> cache;
            bool dispose = false;

            lock (Consumers)
            {
                cache = Cache;

                if (cache == null) {
                    throw new InvalidOperationException("Resource already disposed.");
                }

                if (!Consumers.Remove(borrowed)) {
                    throw new InvalidOperationException("Resource not owned by this client.");
                }

                if (Consumers.Count == 0 && cache.IsValueCreated)
                {
                    dispose = true;
                    Cache = null;
                }
            }

            if (dispose)
            {
                IDisposable disposable = cache.Value as IDisposable;

                if (disposable != null) {
                    disposable.Dispose();
                }
            }
        }
    }
}