using System;

namespace Kirkin.Transactions
{
    /// <summary>
    /// Reflection-based transaction implementation which
    /// creates a snapshot of the object's properties at
    /// initialisation, and rolls back any changes when
    /// disposed, unless preceded by a call to Commit.
    /// </summary>
    public abstract class PropertyTrackingTransaction : IDisposable
    {
        private const int COMMITTED = 1;
        private const int DISPOSED = -1;

        /// <summary>
        /// State of this instance.
        /// Zero when uninitialised.
        /// COMMITTED after the call to Commit but before Dispose.
        /// DISPOSED after call to Dispose.
        /// </summary>
        protected int State = 0;

        // Prevent external overriding (hence sealed Dispose).
        internal PropertyTrackingTransaction()
        {
        }

        /// <summary>
        /// Commits the changes made since this transaction was initialised.
        /// </summary>
        public void Commit()
        {
            if (State == DISPOSED) throw new ObjectDisposedException(GetType().Name);
            if (State == COMMITTED) throw new InvalidOperationException("Already committed.");

            State = COMMITTED;
        }

        /// <summary>
        /// Rolls back the changes made since this transaction was initialised.
        /// </summary>
        protected abstract void Rollback();

        /// <summary>
        /// Rolls back the changes made since this transaction was initialised, unless Commit was previously called.
        /// </summary>
        public void Dispose()
        {
            if (State == DISPOSED) return;

            if (State != COMMITTED) {
                Rollback();
            }

            State = DISPOSED;
        }
    }
}