using System;
using System.Threading;

namespace Kirkin.Threading.Locks
{
    /// <summary>
    /// IReaderWriterLock implementation which uses
    /// a ReaderWriterLockSlim under the covers.
    /// </summary>
    public sealed class ReaderWriterSyncLock : IReaderWriterLock
    {
        /// <summary>
        /// Lock object specified when this instance was created.
        /// </summary>
        public ReaderWriterLockSlim Lock { get; }

        /// <summary>
        /// True if this instance owns and manages
        /// (i.e. disposes of) the underlying lock object.
        /// </summary>
        public bool OwnsLock { get; }

        /// <summary>
        /// Creates a new instance of ReaderWriterSyncLock.
        /// </summary>
        public ReaderWriterSyncLock()
            : this(new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion), true)
        {

        }

        /// <summary>
        /// Creates a new instance of ReaderWriterSyncLock.
        /// </summary>
        public ReaderWriterSyncLock(ReaderWriterLockSlim @lock, bool ownsLock)
        {
            Lock = @lock;
            OwnsLock = ownsLock;
        }

        /// <summary>
        /// Acquires a read lock and returns an
        /// object which releases it when disposed.
        /// </summary>
        public IDisposable ReadLock()
        {
            // Common case optimisation.
            if (Lock.IsReadLockHeld || Lock.IsWriteLockHeld)
            {
                // Null refs can be used by *using* statements.
                return null;
            }

            Lock.EnterReadLock();

            return Disposable.Create(Lock, l => l.ExitReadLock());
        }

        /// <summary>
        /// Acquires a write lock and returns an
        /// object which releases it when disposed.
        /// </summary>
        public IDisposable WriteLock()
        {
            // Common case optimisation.
            if (Lock.IsWriteLockHeld)
            {
                // Null refs can be used by *using* statements.
                return null;
            }

            Lock.EnterWriteLock();

            return Disposable.Create(Lock, l => l.ExitWriteLock());
        }

        /// <summary>
        /// Releases all resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            if (OwnsLock)
            {
                Lock.Dispose();
            }
        }
    }
}