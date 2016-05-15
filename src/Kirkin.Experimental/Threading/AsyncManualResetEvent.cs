// This is *not* original work by Kirill Shlenskiy.
// It is derived from PFX team's blog post available at
// http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266920.aspx

#if !NET_40

using System.Threading;
using System.Threading.Tasks;

using Kirkin.Threading.Tasks;

namespace Kirkin.Threading
{
    /// <summary>
    /// Async analogue of <see cref="ManualResetEventSlim"/>.
    /// </summary>
    public class AsyncManualResetEvent
    {
        private TaskCompletionSource<bool> m_tcs = new TaskCompletionSource<bool>();

        /// <summary>
        /// Creates a new <see cref="AsyncManualResetEvent"/> in an unsignaled state.
        /// </summary>
        public AsyncManualResetEvent()
        {
        }

        /// <summary>
        /// Creates a new <see cref="AsyncManualResetEvent"/> instance with the given initial state.
        /// </summary>
        /// <param name="initialState">True to set the initial state signaled.</param>
        public AsyncManualResetEvent(bool initialState)
        {
            if (initialState) {
                Set();
            }
        }

        /// <summary>
        /// Asynchronously waits until the current <see cref="AsyncManualResetEvent"/> is set.
        /// </summary>
        public async Task WaitAsync()
        {
            // await + ConfigureAwait(false) enables asynchronous completion with behaviour
            // similar to m_tcs.Task.ContinueWith(t => t).Unwrap(), but better perf.
            await m_tcs.Task.ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously waits until the current <see cref="AsyncManualResetEvent"/>
        /// is set or the given <see cref="CancellationToken"/> is signaled.
        /// </summary>
        public Task WaitAsync(CancellationToken ct)
        {
            return WaitAsync().WithCancellation(ct);
        }

        /// <summary>
        /// Sets the state of the event to signaled, which allows one or more waiters to proceed.
        /// </summary>
        public void Set()
        {
            m_tcs.TrySetResult(true);
        }

        /// <summary>
        /// Sets the state of the event to nonsignaled, which causes
        /// future waiters to suspend until <see cref="Set()"/> is called.
        /// </summary>
        public void Reset()
        {
            while (true)
            {
                TaskCompletionSource<bool> tcs = m_tcs;

                if (!tcs.Task.IsCompleted ||
                    Interlocked.CompareExchange(ref m_tcs, new TaskCompletionSource<bool>(), tcs) == tcs)
                {
                    return;
                }
            }
        }
    }
}

#endif