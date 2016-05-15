using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Kirkin.Threading.Tasks
{
    /// <summary>
    /// Container for a task which auto-completes
    /// after the specified amount of time.
    /// </summary>
    internal sealed class DelayTaskSource : IDisposable
    {
        private readonly TaskCompletionSource<bool> TaskCompletionSource = new TaskCompletionSource<bool>();
        private Timer Timer;

        /// <summary>
        /// Returns a Task whose Result will complete
        /// once the specified time period has elapsed,
        /// or canceled as soon as the source is disposed.
        /// </summary>
        public Task DelayTask
        {
            get
            {
                return TaskCompletionSource.Task;
            }
        }

        /// <summary>
        /// Creates a new container for a promise-style task which
        /// auto-completes itself after the specified amount of time.
        /// </summary>
        public DelayTaskSource(TimeSpan completeAfter)
        {
            Timer = new Timer(
                state =>
                {
                    var self = (DelayTaskSource)state;

                    self.TaskCompletionSource.TrySetResult(true);
                    self.Dispose(); // Dispose of the Timer.
                }
                , this
                , (int)completeAfter.TotalMilliseconds
                , Timeout.Infinite
            );
        }

        /// <summary>
        /// Causes the task to transition to canceled state, unless the task
        /// has already completed due to the specified period having elapsed.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
        public void Dispose()
        {
            TaskCompletionSource.TrySetCanceled();

            var timer = Interlocked.Exchange(ref Timer, null);

            if (timer != null) {
                timer.Dispose();
            }
        }
    }
}