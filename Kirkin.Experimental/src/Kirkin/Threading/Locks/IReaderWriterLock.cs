using System;

namespace Kirkin.Threading.Locks
{
    /// <summary>
    /// Contract for Reader-Writer locks.
    /// </summary>
    public interface IReaderWriterLock : IDisposable
    {
        /// <summary>
        /// Acquires a read lock and returns an
        /// object which releases it when disposed.
        /// </summary>
        IDisposable ReadLock();

        /// <summary>
        /// Acquires a write lock and returns an
        /// object which releases it when disposed.
        /// </summary>
        IDisposable WriteLock();
    }
}