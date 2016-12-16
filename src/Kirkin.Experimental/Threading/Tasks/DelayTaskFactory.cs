using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Kirkin.Threading.Tasks
{
    /// <summary>
    /// Factory for promise-style tasks which auto-complete
    /// after the specified amount of time.
    /// Designed for optimized timer reuse in scenarios
    /// with high incidence of delay task cancellation.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    internal sealed class DelayTaskFactory
    {
        private static readonly Task<bool> s_completedTask;

        static DelayTaskFactory()
        {
#if !NET_40
            s_completedTask = Task.FromResult(true);
#else
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(true);
            s_completedTask = tcs.Task;
#endif
        }

        private readonly object Lock = new object();
        private Timer Timer; // Self-disposing.
        private TaskCompletionSource<bool> TaskCompletionSource;

        /// <summary>
        /// Determines the behaviour of this instance in the event that a Restart 
        /// request is made before the previously scheduled delay has completed.
        /// The default is Cancel.
        /// </summary>
        public DelayTaskCancellationMode CancellationMode { get; }

        public DelayTaskFactory()
            : this(DelayTaskCancellationMode.Cancel)
        {
        }

        public DelayTaskFactory(DelayTaskCancellationMode cancellationMode)
        {
            CancellationMode = cancellationMode;
        }

        /// <summary>
        /// Cancels previous delay task (if any), and returns a newly started delay task.
        /// </summary>
        public Task<bool> Restart(TimeSpan completeAfter)
        {
            lock (Lock)
            {
                if (TaskCompletionSource != null) {
                    DelayTaskCancellation.Apply(TaskCompletionSource, CancellationMode);
                }

                if (completeAfter == TimeSpan.Zero)
                {
                    if (Timer != null)
                    {
                        Timer.Dispose();
                        Timer = null;
                    }

                    return s_completedTask;
                }

                TaskCompletionSource = new TaskCompletionSource<bool>();

                // If there is a pending operation, its timer is
                // reused. Otherwise, a new one will be created.
                StartOrChangeTimer(completeAfter);

                return TaskCompletionSource.Task;
            }
        }

        /// <summary>
        /// Cancels any pending delay tasks.
        /// </summary>
        public void Cancel()
        {
            lock (Lock)
            {
                if (TaskCompletionSource != null)
                {
                    DelayTaskCancellation.Apply(TaskCompletionSource, CancellationMode);
                    TaskCompletionSource = null;
                }

                if (Timer != null)
                {
                    Timer.Dispose();
                    Timer = null;
                }
            }
        }

        private void StartOrChangeTimer(TimeSpan completeAfter)
        {
            int dueTime = (int)completeAfter.TotalMilliseconds;

            if (Timer != null)
            {
                Timer.Change(dueTime, Timeout.Infinite);
            }
            else
            {
                Timer = new Timer(
                    state =>
                    {
                        var self = (DelayTaskFactory)state;

                        lock (self.Lock)
                        {
                            if (self.TaskCompletionSource != null)
                            {
                                self.TaskCompletionSource.SetResult(true);
                                self.TaskCompletionSource = null;
                            }

                            if (self.Timer != null)
                            {
                                self.Timer.Dispose();
                                self.Timer = null;
                            }
                        }
                    }
                    , this
                    , dueTime
                    , Timeout.Infinite
                );
            }
        }
    }
}