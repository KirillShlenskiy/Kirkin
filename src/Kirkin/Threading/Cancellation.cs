using System.Threading;

namespace Kirkin.Threading
{
    /// <summary>
    /// Utilities for working with CancellationTokenSources.
    /// </summary>
    public static class Cancellation
    {
        /// <summary>
        /// Creates a new <see cref="CancellationTokenSource"/> instance,
        /// replaces the <see cref="CancellationTokenSource"/> at the given
        /// location with it, cancels the old instance if it was not
        /// a null reference and returns the new instance's token.
        /// </summary>
        public static CancellationToken Renew(ref CancellationTokenSource location)
        {
            CancellationTokenSource newCts = new CancellationTokenSource();
            CancellationTokenSource oldCts = Interlocked.Exchange(ref location, newCts);

            if (oldCts != null) {
                oldCts.Cancel();
            }

            return newCts.Token;
        }

        /// <summary>
        /// Replaces the <see cref="CancellationTokenSource"/> at
        /// the given location with a null reference and cancels
        /// the old instance if it was not a null reference.
        /// Returns true if the source was successfully replaced.
        /// </summary>
        public static bool Reset(ref CancellationTokenSource location)
        {
            CancellationTokenSource oldCts = Interlocked.Exchange(ref location, null);

            if (oldCts != null)
            {
                oldCts.Cancel();

                return true;
            }

            return false;
        }
    }
}