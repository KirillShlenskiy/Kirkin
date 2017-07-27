using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kirkin.Threading
{
    public sealed class AsyncLock
    {
        private TaskCompletionSource<bool> _tcs;

        public async Task<AsyncLockReleaser> EnterAsync()
        {
            TaskCompletionSource<bool> newTcs = new TaskCompletionSource<bool>();
            TaskCompletionSource<bool> oldTcs = Interlocked.Exchange(ref _tcs, newTcs);

            if (oldTcs != null)
            {
                // TODO: async completion?
                await oldTcs.Task.ConfigureAwait(false);
            }

            return new AsyncLockReleaser(newTcs);
        }
    }

    public struct AsyncLockReleaser : IDisposable
    {
        private readonly TaskCompletionSource<bool> _tcs;

        internal AsyncLockReleaser(TaskCompletionSource<bool> tcs)
        {
            _tcs = tcs;
        }

        public void Dispose()
        {
            _tcs.SetResult(true);
        }
    }
}