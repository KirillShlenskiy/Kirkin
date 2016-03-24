#if NET_45

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kirkin.Threading.Tasks
{
    /// <summary>
    /// Task extensions.
    /// </summary>
    public static class TaskEx
    {
        /// <summary>
        /// Allows abandoning tasks which do not natively
        /// support cancellation. Use with caution.
        /// </summary>
        public static async Task WithCancellation(this Task task, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            using (ct.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs, false))
            {
                if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
                {
                    // Cancellation task completed first.
                    // We are abandoning the original task.
                    throw new OperationCanceledException(ct);
                }
            }

            // Task completed: observe exceptions.
            await task.ConfigureAwait(false);
        }

        /// <summary>
        /// Allows abandoning tasks which do not natively
        /// support cancellation. Use with caution.
        /// </summary>
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            using (ct.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs, false))
            {
                if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
                {
                    // Cancellation task completed first.
                    // We are abandoning the original task.
                    throw new OperationCanceledException(ct);
                }
            }

            // Task completed: synchronously return result or propagate exceptions.
            return await task.ConfigureAwait(false);
        }
    }
}

#endif