using System;
using System.Diagnostics;
using System.Threading;

namespace Kirkin.Threading
{
    /// <summary>
    /// Blocking synchronization primitive primarily used for inter-process communication.
    /// </summary>
    public sealed class NamedMutex : IDisposable
    {
        /// <summary>
        /// Enters the mutex with the given name.
        /// </summary>
        public static NamedMutex Enter(string mutexName)
        {
            Mutex mutex = null;

            try
            {
                mutex = new Mutex(false, mutexName);

                // Enter mutex.
                if (!mutex.WaitOne(0))
                {
                    Debug.Print($"Another thread owns {mutexName}. Waiting for it to be released ...");

                    if (!mutex.WaitOne(TimeSpan.FromSeconds(5))) {
                        throw new TimeoutException($"Timeout elapsed while trying to enter {mutexName}.");
                    }
                }
            }
            catch (AbandonedMutexException)
            {
                // Mutex entered.
            }
            catch
            {
                // Bad news.
                mutex?.Dispose();

                throw;
            }

            return new NamedMutex(mutex);
        }

        private readonly Mutex _mutex;

        private NamedMutex(Mutex mutex)
        {
            _mutex = mutex;
        }

        /// <summary>
        /// Releases this mutex.
        /// </summary>
        public void Dispose()
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
        }
    }
}