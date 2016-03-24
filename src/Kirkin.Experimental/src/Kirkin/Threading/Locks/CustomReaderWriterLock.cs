using System;

namespace Kirkin.Threading.Locks
{
    /// <summary>
    /// Custom implementation of a reader-writer lock
    /// where the client specifies the individual locks
    /// to use in read and write scenarios.
    /// </summary>
    public class CustomReaderWriterLock : IReaderWriterLock
    {
        private readonly ILock RLock;
        private readonly ILock WLock;

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="readLock">Read lock or null.</param>
        /// <param name="writeLock">Write lock or null.</param>
        public CustomReaderWriterLock(ILock readLock, ILock writeLock)
        {
            RLock = readLock;
            WLock = writeLock;
        }

        /// <summary>
        /// Acquires a read lock and returns an
        /// object which releases it when disposed.
        /// </summary>
        public IDisposable ReadLock()
        {
            if (RLock == null)
            {
                return Disposable.Empty;
            }

            return RLock.Lock();
        }

        /// <summary>
        /// Acquires a write lock and returns an
        /// object which releases it when disposed.
        /// </summary>
        public IDisposable WriteLock()
        {
            if (WLock == null)
            {
                return Disposable.Empty;
            }

            return WLock.Lock();
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Dispose()
        {   
        }
    }
}