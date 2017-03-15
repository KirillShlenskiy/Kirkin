#if !NET_40

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Kirkin.Collections.Async
{
    /// <summary>
    /// Async handover which adds items asynchronously
    /// and does not allow more than a certain number
    /// of items in the local cache.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public sealed class BoundedCapacityAsyncHandover<T> : AsyncHandoverBase<T>
    {
        // Semaphore which will throttle the Add methods
        // once the item cache reaches its bounded capacity.
        // IDisposable, but as long as we don't
        // bother AvailableWaitHandle, we will be fine.
        private readonly SemaphoreSlim CapacitySemaphore;

        /// <summary>
        /// Sets the max capacity of the item cache.
        /// </summary>
        public int BoundedCapacity { get; }

        /// <summary>
        /// Creates a new instance of the handover.
        /// </summary>
        public BoundedCapacityAsyncHandover(int boundedCapacity)
        {
            if (boundedCapacity <= 0) throw new ArgumentOutOfRangeException("boundedCapacity");

            BoundedCapacity = boundedCapacity;
            CapacitySemaphore = new SemaphoreSlim(boundedCapacity, boundedCapacity);
        }

        /// <summary>
        /// Creates a new instance of the handover.
        /// </summary>
        public BoundedCapacityAsyncHandover(IProducerConsumerCollection<T> inner, int boundedCapacity)
            : base(inner)
        {
            if (boundedCapacity <= 0) throw new ArgumentOutOfRangeException("boundedCapacity");

            if (inner.Count > boundedCapacity) {
                throw new ArgumentException("Collection count exceeds the bounded capacity.");
            }

            BoundedCapacity = boundedCapacity;
            CapacitySemaphore = new SemaphoreSlim(boundedCapacity - inner.Count, boundedCapacity);
        }

        /// <summary>
        /// Adds an item to the pending queue or hands
        /// it over to the waiting TryTakeAsync caller.
        /// Use this method with bounded collections.
        /// </summary>
        public Task AddAsync(T item)
        {
            return AddAsync(item, CancellationToken.None);
        }

        /// <summary>
        /// Adds an item to the pending queue or hands
        /// it over to the waiting TryTakeAsync caller.
        /// Use this method with bounded collections.
        /// </summary>
        public async Task AddAsync(T item, CancellationToken ct)
        {
            if (!await TryAddAsync(item, ct).ConfigureAwait(false))
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
        /// Use this method with bounded collections.
        /// </summary>
        public Task<bool> TryAddAsync(T item)
        {
            return TryAddAsync(item, CancellationToken.None);
        }

        /// <summary>
        /// Adds an item to the pending queue or hands
        /// it over to the waiting TryTakeAsync caller.
        /// Use this method with bounded collections.
        /// </summary>
        public async Task<bool> TryAddAsync(T item, CancellationToken ct)
        {
            // Pre-enter the semaphore. We need to do this outside
            // of the main mutex in order to avoid async deadlocks.
            await CapacitySemaphore.WaitAsync(ct).ConfigureAwait(false);

            // In most cases we want the semaphore released at the
            // end of the method. The only exception to this is if
            // the method ultimately ends up adding to the item cache.
            bool needToReleaseCapacitySemaphore = true;

            try
            {
                bool result = TryAddInternal(item, out bool itemAddedToCache);

                if (itemAddedToCache) {
                    // This is the only case where we don't want to release.
                    needToReleaseCapacitySemaphore = false;
                }

                return result;
            }
            finally
            {
                if (needToReleaseCapacitySemaphore) {
                    CapacitySemaphore.Release();
                }
            }
        }

        /// <summary>
        /// Attempts to immediately remove and return
        /// the object at the beginning of the queue.
        /// </summary>
        public override bool TryTakeImmediately(out T item)
        {
            if (base.TryTakeImmediately(out item))
            {
                CapacitySemaphore.Release();

                return true;
            }

            return false;
        }
    }
}

#endif