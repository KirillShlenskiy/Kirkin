#if !NET_40

using System;
using System.Collections.Concurrent;

namespace Kirkin.Collections.Async
{
    /// <summary>
    /// Simple thread-safe async producer-consumer
    /// collection similar to BlockingCollection{T}.
    /// </summary>
    public sealed class AsyncHandover<T> : AsyncHandoverBase<T>
    {
        /// <summary>
        /// Creates a new instance of the collection.
        /// </summary>
        public AsyncHandover()
        {
        }

        /// <summary>
        /// Creates a new instance of the collection.
        /// </summary>
        public AsyncHandover(IProducerConsumerCollection<T> inner)
            : base(inner)
        {
        }

        /// <summary>
        /// Adds an item to the pending queue or hands
        /// it over to the waiting TryTakeAsync caller.
        /// </summary>
        public void Add(T item)
        {
            if (!TryAdd(item))
            {
                if (!AllowsAdding) {
                    throw new InvalidOperationException("Cannot add items after CompleteAdding was called.");
                }

                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Adds an item to the pending queue or hands
        /// it over to the waiting TryTakeAsync caller.
        /// </summary>
        public bool TryAdd(T item)
        {
            // Ignored.
            bool itemAddedToCache = false;

            return TryAddInternal(item, out itemAddedToCache);
        }
    }
}

#endif