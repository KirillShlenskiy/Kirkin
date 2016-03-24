using System;
using System.Threading;

namespace Kirkin.Threading.Locks
{
    /// <summary>
    /// Lock implementation which uses
    /// a Monitor under the covers.
    /// </summary>
    public class MonitorLock : ILock
    {
        private object LockObject;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public MonitorLock() : this(new object())
        {

        }

        /// <summary>
        /// Creates a new instance using the specified lock object.
        /// </summary>
        public MonitorLock(object lockObject)
        {
            LockObject = lockObject;
        }

        /// <summary>
        /// Acquires a lock and returns an
        /// object which releases it when disposed.
        /// </summary>
        public IDisposable Lock()
        {
            var lockObject = LockObject;

            if (lockObject == null)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            Monitor.Enter(lockObject);

            return Disposable.Create(lockObject, Monitor.Exit);
        }

        /// <summary>
        /// Releases the resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            LockObject = null;
        }
    }
}