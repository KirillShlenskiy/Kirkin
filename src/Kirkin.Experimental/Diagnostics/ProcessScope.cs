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
        const int STATE_ACTIVE = 0;
        const int STATE_DISPOSED = 1;
        const int STATE_COMPLETED = 2;

        private readonly EventHandler CurrentDomainExitHandler;
        private int _state = STATE_ACTIVE;

        /// <summary>
        /// <see cref="System.Diagnostics.Process"/> whose lifetime is managed by ths instance.
        /// </summary>
        public Process Process { get; }

        /// <summary>
        /// Returns true if the process was killed as the result of the <see cref="Dispose"/> call.
        /// </summary>
        public bool ForciblyTerminated { get; private set; }

        /// <summary>
        /// Creates a new <see cref="ProcessScope"/> instance.
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
        /// Marks the scope as complete, disabling subsequent <see cref="Dispose"/> calls.
        /// </summary>
        public void Complete()
        {
            if (Interlocked.CompareExchange(ref _state, STATE_COMPLETED, STATE_ACTIVE) == STATE_DISPOSED) {
                throw new ObjectDisposedException(nameof(ProcessScope));
            }
        }

        /// <summary>
        /// Terminates the managed process if necessary and releases any resources held by this instance.
        /// </summary>
        public void Dispose()
        {
            // Even if the below Kill call fails, we don't want this
            // handler left dangling as it won't do anything useful.
            AppDomain.CurrentDomain.ProcessExit -= CurrentDomainExitHandler;

            bool needsDisposing = (Interlocked.CompareExchange(ref _state, STATE_DISPOSED, STATE_ACTIVE) == STATE_ACTIVE);

            if (needsDisposing)
            {
                if (Process.HasExited)
                {
                    // TODO: Determine if this is really necessary.
                    Process.Close();
                }
                else
                {
                    // Setting this flag first to ensure that any clients listening for the Process.Exited
                    // event or waiting on the Process.WaitForExit call will be able to see this value.
                    ForciblyTerminated = true;

                    Process.Kill();
                }
            }
        }
    }
}