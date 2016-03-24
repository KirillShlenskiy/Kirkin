using System;

namespace Kirkin.Threading.Locks
{
    /// <summary>
    /// Contract for basic locks.
    /// </summary>
    public interface ILock : IDisposable
    {
        /// <summary>
        /// Acquires a lock and returns an
        /// object which releases it when disposed.
        /// </summary>
        IDisposable Lock();
    }
}