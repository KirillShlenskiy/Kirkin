using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kirkin.Monitoring
{
    /// <summary>
    /// Reactive, asynchronous entry producer.
    /// </summary>
    internal interface IMonitor<TEntry>
    {
        /// <summary>
        /// Raised when the monitor detects a new entry.
        /// Raised on the thread which calls MonitorAsync if the SynchronizationContext is not null.
        /// </summary>
        event Action<TEntry> EntryRead;

        /// <summary>
        /// Starts monitoring the file optionally checking the given cancellation token.
        /// </summary>
        Task MonitorAsync(CancellationToken ct = default(CancellationToken));
    }
}