#if !NET_40

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kirkin.Collections.Async
{
    /// <summary>
    /// Simple thread-safe async producer-consumer
    /// collection similar to BlockingCollection{T}.
    /// </summary>
    public abstract class AsyncHandoverBase<T>
    {
        #region Fields

        // Queue of items waiting to be handed
        // over at the next call to TryTakeAsync.
        private readonly IProducerConsumerCollection<T> Items;

        // Completion source created by a call
        // to TryTakeAsync when no queued items are
        // available to be immediately consumed. 
        private readonly Queue<TaskCompletionSource<TakeResult<T>>> Waiters
            = new Queue<TaskCompletionSource<TakeResult<T>>>();

        #endregion

        #region Properties

        /// <summary>
        /// True if this collection allows adding.
        /// False if CompleteAdding has been called.
        /// </summary>
        public bool AllowsAdding { get; private set; }

        /// <summary>
        /// Gets the number of materialized items waiting
        /// to be passed to consumers.
        /// If this number is greater than zero, the next
        /// call to TryTakeAsync will complete synchronously.
        /// </summary>
        public int Count
        {
            get
            {
                return Items.Count;
            }
        }

        /// <summary>
        /// Gets the number of TryTakeAsync callers
        /// waiting for the next available item.
        /// </summary>
        public int WaiterCount
        {
            get
            {
                lock (Waiters) {
                    return Waiters.Count;
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the async collection
        /// using a ConcurrentQueue as its backing store.
        /// </summary>
        protected AsyncHandoverBase()
            : this(new ConcurrentQueue<T>())
        {
        }

        /// <summary>
        /// Creates a new instance of the async collection using the
        /// specified producer-consumer collection as its backing store.
        /// </summary>
        protected AsyncHandoverBase(IProducerConsumerCollection<T> inner)
        {
            if (inner == null) throw new ArgumentNullException("inner");

            Items = inner;
            AllowsAdding = true;
        }

        #endregion

        #region Add/TryAdd

        /// <summary>
        /// Hands the item over to the waiters
        /// directly or adds it to the item cache.
        /// </summary>
        protected bool TryAddInternal(T item, out bool itemAddedToCache)
        {
            itemAddedToCache = false;

            while (true)
            {
                TaskCompletionSource<TakeResult<T>> nextWaiter;

                lock (Waiters)
                {
                    if (!AllowsAdding) {
                        return false;
                    }

                    // Resolve the next waiter which
                    // has not been cancelled yet.
                    if (Waiters.Count == 0)
                    {
                        // No waiters. Save for later.
                        if (Items.TryAdd(item))
                        {
                            itemAddedToCache = true;

                            return true;
                        }

                        // Unable to add.
                        return false;
                    }

                    nextWaiter = Waiters.Dequeue();
                }

                // TrySetResult will not do anything for  
                // waiters  which have already been canceled.
                if (nextWaiter.TrySetResult(new TakeResult<T>(item))) {
                    // We have successfully signaled a non-canceled waiter.
                    // Our work here is done.
                    return true;
                }

                // The waiter must have been cancelled.
                // We'll keep retrying until we succeed.
            }
        }

        #endregion

        #region TryTake

        /// <summary>
        /// Attempts to immediately remove and return
        /// the object at the beginning of the queue.
        /// </summary>
        public virtual bool TryTakeImmediately(out T item)
        {
            return Items.TryTake(out item);
        }

        /// <summary>
        /// Asynchronously takes the next item.
        /// This method completes synchronously if
        /// an item is available in the queue.
        /// </summary>
        public Task<TakeResult<T>> TryTakeAsync()
        {
            return TryTakeAsync(CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously takes the next item.
        /// This method completes synchronously if
        /// an item is available in the queue.
        /// </summary>
        public Task<TakeResult<T>> TryTakeAsync(CancellationToken ct)
        {
            TaskCompletionSource<TakeResult<T>> waiter;

            lock (Waiters)
            {
                ct.ThrowIfCancellationRequested();

                T item;

                // Check if an item was queued while
                // we were waiting for the mutex.
                if (TryTakeImmediately(out item)) {
                    return Task.FromResult(new TakeResult<T>(item));
                }

                if (!AllowsAdding) {
                    // CompleteAdding was called. No point waiting for Add.
                    return Task.FromResult(default(TakeResult<T>));
                }

                ct.ThrowIfCancellationRequested();

                // Register async waiter.
                waiter = new TaskCompletionSource<TakeResult<T>>();

                Waiters.Enqueue(waiter);
            }

            // Wait for the next call to Add or CompleteAdding.
            if (!ct.CanBeCanceled) {
                return waiter.Task;
            }

            return TryTakeAsyncImpl(waiter, ct);
        }

        private async Task<TakeResult<T>> TryTakeAsyncImpl(TaskCompletionSource<TakeResult<T>> waiter, CancellationToken ct)
        {
            // Cancellation support.
            // TrySetCanceled will not do anything if it is
            // preempted by a call to TrySetResult, in which 
            // case this method will return successfully.
            // This is crucial to ensure that no items are lost.
            using (ct.Register(state => ((TaskCompletionSource<TakeResult<T>>)state).TrySetCanceled(), waiter, false)) {
                return await waiter.Task.ConfigureAwait(false);
            }
        }

        #endregion

        #region CompleteAdding/Misc

        /// <summary>
        /// Sets the AllowsAdding flag to false
        /// and cancels any pending wait tasks.
        /// Any calls to Add from the point when
        /// CompleteAdding is called will throw.
        /// </summary>
        public void CompleteAdding()
        {
            if (!AllowsAdding)
                return;

            // We do not want to signal inside the lock.
            TaskCompletionSource<TakeResult<T>>[] waitersToSignal = null;

            lock (Waiters)
            {
                // Note: the items already in the queue will
                // remain accessible via a call to TryTakeAsync.
                AllowsAdding = false;

                if (Waiters.Count != 0)
                {
                    // Clear the await queue.
                    waitersToSignal = new TaskCompletionSource<TakeResult<T>>[Waiters.Count];

                    Waiters.CopyTo(waitersToSignal, 0);
                    Waiters.Clear();
                }
            }

            if (waitersToSignal != null)
            {
                // Signal the waiters.
                foreach (TaskCompletionSource<TakeResult<T>> waiter in waitersToSignal)
                {
                    // TrySetResult will not do anything
                    // for waiters which have been cancelled.
                    // They will simply be removed from the queue.
                    waiter.TrySetResult(default(TakeResult<T>));
                }
            }
        }

        #endregion
    }
}

#endif