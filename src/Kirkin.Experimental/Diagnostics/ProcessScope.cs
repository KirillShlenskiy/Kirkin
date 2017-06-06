using System;
using System.Diagnostics;
using System.Threading;

namespace Kirkin.Diagnostics
{
    /// <summary>
    /// Manages the lifetime of the given process, ensuring that the process is reliably
    /// killed if <see cref="IDisposable.Dispose"/> is called before the scope is marked as completed.
    /// Also terminates the process if the current <see cref="AppDomain"/> exits before the scope is completed.
    /// </summary>
    public sealed class ProcessScope : IDisposable
    {
        ///// <summary>
        ///// Starts a new <see cref="System.Diagnostics.Process"/> and returns a
        ///// <see cref="ProcessScope"/> instance which will manage its lifetime.
        ///// </summary>
        //public ProcessScope Start(ProcessStartInfo startInfo)
        //{
        //    Process process = Process.Start(startInfo);

        //    return new ProcessScope(process);
        //}

        const int STATE_ACTIVE = 0;
        const int STATE_COMPLETED = 1;
        const int STATE_TERMINATED = 2;

        private readonly EventHandler CurrentDomainExitHandler;
        private int _state = STATE_ACTIVE;

        /// <summary>
        /// <see cref="System.Diagnostics.Process"/> whose lifetime is managed by ths instance.
        /// </summary>
        public Process Process { get; }

        /// <summary>
        /// Returns true if the process was killed as the result of the <see cref="Dispose"/> call.
        /// </summary>
        public bool ForciblyTerminated
        {
            get
            {
                return _state == STATE_TERMINATED;
            }
        }

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
        /// Marks the scope as successfully completed, disabling subsequent <see cref="Dispose"/> calls.
        /// </summary>
        public void Complete()
        {
            // Multiple calls to Complete are fine.
            if (Interlocked.CompareExchange(ref _state, STATE_COMPLETED, STATE_ACTIVE) == STATE_TERMINATED) {
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

            if (Interlocked.CompareExchange(ref _state, STATE_TERMINATED, STATE_ACTIVE) == STATE_ACTIVE)
            {
                // The HasExited check reduces exceptions in situations where the
                // process has already completed, but the client hasn't yet had the
                // chance to call Complete. In that case another Dispose call
                // can come in as the result of user cancellation, for instance.
                if (!Process.HasExited) {
                    Process.Kill();
                }
            }
        }
    }
}