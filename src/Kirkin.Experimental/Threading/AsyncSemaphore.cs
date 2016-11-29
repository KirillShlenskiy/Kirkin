using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kirkin.Threading
{
    /// <summary>
    /// Limits the number of threads that can access a resource concurrently.
    /// </summary>
    public sealed class AsyncSemaphore
    {
        private static readonly Task s_completedTask = Task.FromResult(true);

        private readonly Queue<TaskCompletionSource<bool>> Waiters = new Queue<TaskCompletionSource<bool>>();
        private int _count;

        public int CurrentCount
        {
            get
            {
                return _count;
            }
        }

        public int MaxCount { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSemaphore"/> class.
        /// </summary>
        public AsyncSemaphore(int initialCount)
            : this(initialCount, int.MaxValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSemaphore"/> class.
        /// </summary>
        public AsyncSemaphore(int initialCount, int maxCount)
        {
            _count = initialCount;
            MaxCount = maxCount;
        }

        /// <summary>
        /// Asynchronously waits to enter the <see cref="AsyncSemaphore"/>,
        /// while observing the given <see cref="CancellationToken"/>.
        /// </summary>
        public Task WaitAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (Waiters)
            {
                if (_count > 0)
                {
                    _count--;

                    return s_completedTask;
                }

                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>(cancellationToken);

                if (cancellationToken.CanBeCanceled)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    cancellationToken.Register(
                        state => ((TaskCompletionSource<bool>)state).TrySetCanceled(),
                        tcs,
                        useSynchronizationContext: false
                    );
                }

                Waiters.Enqueue(tcs);

                return tcs.Task;
            }
        }

        // <summary>
        /// Exits the <see cref="AsyncSemaphore"/> a specified number of times.
        /// </summary>
        public void Release(int releaseCount = 1)
        {
            if (releaseCount < 1) throw new ArgumentOutOfRangeException(nameof(releaseCount));

            lock (Waiters)
            {
                ValidateReleaseCount(releaseCount);

                for (int i = 0; i < releaseCount; i++)
                {
                    bool waiterSet = false;

                    while (Waiters.Count != 0)
                    {
                        TaskCompletionSource<bool> tcs = Waiters.Dequeue();

                        if (tcs.TrySetResult(true)) // Synchronous completion ok?
                        {
                            waiterSet = true;

                            break;
                        }
                    }

                    if (!waiterSet)
                    {
                        // Validate release count. We can't compute this value before the
                        // loop due to possibility of races with cancellable waiters.
                        int newCount = _count + 1;

                        if (newCount > MaxCount) {
                            ThrowSemaphoreCountExceeded();
                        }

                        _count = newCount;
                    }
                }
            }
        }

        private void ValidateReleaseCount(int releaseCount)
        {
            int bestGuessNewCount = _count + releaseCount;

            // Waiters can race with us, but this is a reasonable
            // fail-safe in simple sequential scenarios.
            foreach (TaskCompletionSource<bool> tcs in Waiters)
            {
                if (!tcs.Task.IsCompleted) {
                    bestGuessNewCount--;
                }
            }

            if (bestGuessNewCount > MaxCount) {
                ThrowSemaphoreCountExceeded();
            }
        }

        private static void ThrowSemaphoreCountExceeded()
        {
            throw new InvalidOperationException("Semaphore count after Release exceeds max count.");
        }
    }
}