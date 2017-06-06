using System;
using System.Diagnostics;
using System.Threading;

namespace Kirkin.Diagnostics
{
    /// <summary>
    /// Manages the lifetime of the given process, ensuring that the process is reliably
    /// killed if <see cref="IDisposable.Dispose"/> is called. Also terminates the process
    /// if the current <see cref="AppDomain"/> exits while the process is still running.
    /// </summary>
    public sealed class ProcessScope : IDisposable
    {
        private readonly EventHandler CurrentDomainExitHandler;
        private int _disposed;

        /// <summary>
        /// <see cref="System.Diagnostics.Process"/> whose lifetime is managed by ths instance.
        /// </summary>
        public Process Process { get; }

        /// <summary>
        /// Returns true if the process was killed as the result of the <see cref="Dispose"/> call.
        /// </summary>
        public bool ForciblyTerminated { get; private set; }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="process">Process whose lifetime will be managed by this instance.</param>
        public ProcessScope(Process process)
        {
            Process = process;
            CurrentDomainExitHandler = (s, e) => Dispose();

            // Ensure that the managed process is killed when this domain exits.
            AppDomain.CurrentDomain.ProcessExit += CurrentDomainExitHandler;
        }

        /// <summary>
        /// Terminates the managed process if necessary and releases any resources held by this instance.
        /// </summary>
        public void Dispose()
        {
            bool needsDisposing = (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0);

            if (needsDisposing)
            {
                if (Process.HasExited)
                {
                    Process.Close();
                }
                else
                {
                    Process.Kill();

                    ForciblyTerminated = true;
                }

                AppDomain.CurrentDomain.ProcessExit -= CurrentDomainExitHandler;
            }
        }
    }
}