using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kirkin.Threading
{
    /// <summary>
    /// Fast non-cancelable async lock.
    /// </summary>
    public sealed class AsyncLock
    {
        private TaskCompletionSource<bool> _tcs;

        /// <summary>
        /// Enters the lock asynchronously and returns an <see cref="IDisposable"/>
        /// object which releases the lock when disposed.
        /// </summary>
        public async Task<AsyncLockReleaser> EnterAsync()
        {
            TaskCompletionSource<bool> newTcs = new TaskCompletionSource<bool>();
            TaskCompletionSource<bool> oldTcs = Interlocked.Exchange(ref _tcs, newTcs);

            if (oldTcs != null) {
                await oldTcs.Task.ConfigureAwait(false);
            }

            return new AsyncLockReleaser(newTcs);
        }
    }

    /// <summary>
    /// <see cref="AsyncLock"/> releaser structure.
    /// </summary>
    public sealed class AsyncLockReleaser : IDisposable
    {
        private readonly TaskCompletionSource<bool> _tcs;

        internal AsyncLockReleaser(TaskCompletionSource<bool> tcs)
        {
            _tcs = tcs;
        }

        /// <summary>
        /// Releases the <see cref="AsyncLock"/>.
        /// </summary>
        public void Dispose()
        {
            _tcs.TrySetResult(true);
        }
    }
}