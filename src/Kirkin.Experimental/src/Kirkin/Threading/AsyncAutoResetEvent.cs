#if NET_45

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kirkin.Threading
{
    /// <summary>
    /// Async analogue of <see cref="System.Threading.AutoResetEvent"/>.
    /// </summary>
    public class AsyncAutoResetEvent
    {
        private readonly static Task CompletedTask = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> Waits;

        /// <summary>
        /// Gets the state of this instance.
        /// </summary>
        public bool Signaled { get; private set; }

        /// <summary>
        /// Creates a new instance which supports an unlimited number of waiters.
        /// </summary>
        public AsyncAutoResetEvent()
        {
            Waits = new Queue<TaskCompletionSource<bool>>();
        }

        /// <summary>
        /// Creates a new instance which supports the given number of waiters.
        /// </summary>
        public AsyncAutoResetEvent(int capacity)
        {
            Waits = new Queue<TaskCompletionSource<bool>>(capacity);
        }

        /// <summary>
        /// Asynchronously waits until the current <see cref="AsyncAutoResetEvent"/> is set.
        /// </summary>
        public Task WaitAsync()
        {
            lock (Waits)
            {
                if (Signaled)
                {
                    Signaled = false;

                    return CompletedTask;
                }

                var tcs = new TaskCompletionSource<bool>();

                Waits.Enqueue(tcs);

                return tcs.Task;
            }
        }

        /// <summary>
        /// Sets the state of the event to signaled, which causes
        /// a single (queued or future) waiter to proceed.
        /// </summary>
        public void Set()
        {
            TaskCompletionSource<bool> toRelease = null;

            lock (Waits)
            {
                if (Waits.Count != 0)
                {
                    toRelease = Waits.Dequeue();
                }
                else if (!Signaled)
                {
                    Signaled = true;
                }
            }

            if (toRelease != null) {
                toRelease.SetResult(true);
            }
        }
    }
}

#endif