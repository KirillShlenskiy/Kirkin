using System;
using System.Diagnostics;
using System.Threading;

namespace Kirkin.Diagnostics
{
    /// <summary>
    /// Wraps a <see cref="Process"/> instance and ensures it is
    /// reliably cleaned up if the parent (this process) is terminated.
    /// </summary>
    internal sealed class ProcessLifetimeManager : IDisposable
    {
        private Process _process;
        private EventHandler processExitHandler;

        public bool ProcessExited { get; private set; }

        /// <summary>
        /// Child process wrapped by this instance.
        /// </summary>
        public Process Process
        {
            get
            {
                if (_process == null) {
                    throw new ObjectDisposedException(nameof(ProcessLifetimeManager));
                }

                return _process;
            }
        }

        /// <summary>
        /// Creates a new <see cref="ProcessLifetimeManager"/> instance.
        /// </summary>
        public ProcessLifetimeManager(Process process)
        {
            if (process == null) throw new ArgumentNullException(nameof(process));

            _process = process;

            _process.EnableRaisingEvents = true;

            process.Exited += delegate
            {
                ProcessExited = true;

                Dispose();
            };

            processExitHandler = delegate {
                Dispose();
            };

            // Ensure the child process is killed if the parent exits.
            AppDomain.CurrentDomain.ProcessExit += processExitHandler;
        }

        /// <summary>
        /// Reliably terminates the child process and cleans up the resources associated with it.
        /// </summary>
        public void Dispose()
        {
            AppDomain.CurrentDomain.ProcessExit -= processExitHandler;
            Process p = Interlocked.Exchange(ref _process, null);

            if (p != null)
            {
                if (ProcessExited)
                {
                    p.Close();
                }
                else
                {
                    p.Kill();
                }
            }
        }
    }
}