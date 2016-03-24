using System;

namespace Kirkin.Threading.Locks
{
    /// <summary>
    /// Lock implementation which
    /// does not perform any locking.
    /// </summary>
    public class NullLock : ILock
    {
        /// <summary>
        /// Returns an object which
        /// does nothing when disposed.
        /// </summary>
        public IDisposable Lock()
        {
            // Could technically be replaced with
            // an actual null reference for use
            // with *using* statements.
            return Disposable.Empty;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Dispose()
        {
            
        }
    }
}