﻿using System;
using System.Diagnostics;
using System.Threading;

namespace Kirkin.Diagnostics
{
    /// <summary>
    /// Manages the lifetime of the given process, ensuring that the process is reliably
    /// killed if <see cref="IDisposable.Dispose"/> is called before the scope is marked as completed.
    /// Also terminates the process if the current <see cref="AppDomain"/> exits before the scope is completed.
    /// </summary>
    [Obsolete("May not reliably kill the associated process. Consider using ChildProcess instead.")]
    public sealed class ProcessScope : IDisposable
    {
        const int STATE_ACTIVE = 0;
        const int STATE_COMPLETED = 1;
        const int STATE_DISPOSED = 2;

        private readonly EventHandler CurrentDomainExitHandler;
        private int _state = STATE_ACTIVE;

        /// <summary>
        /// <see cref="System.Diagnostics.Process"/> whose lifetime is managed by ths instance.
        /// </summary>
        public Process Process { get; }

        /// <summary>
        /// Returns true if <see cref="Dispose"/> has been called.
        /// </summary>
        public bool Disposed
        {
            get
            {
                return _state == STATE_DISPOSED;
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
        /// Returns false if the scope is already disposed. Otherwise returns true.
        /// </summary>
        public bool Complete()
        {
            // Multiple calls to Complete are fine.
            int state = Interlocked.CompareExchange(ref _state, STATE_COMPLETED, STATE_ACTIVE);

            if (state == STATE_DISPOSED) {
                return false;
            }

            AppDomain.CurrentDomain.ProcessExit -= CurrentDomainExitHandler;

            return true;
        }

        /// <summary>
        /// Terminates the managed process (if necessary).
        /// </summary>
        public void Dispose()
        {
            int state = Interlocked.Exchange(ref _state, STATE_DISPOSED);

            if (state != STATE_DISPOSED)
            {
                // Even if the below Kill call fails, we don't want this
                // handler left dangling as it won't do anything useful.
                AppDomain.CurrentDomain.ProcessExit -= CurrentDomainExitHandler;

                // The HasExited check serves to reduce exceptions in situations where
                // the process has already completed, but the client hasn't yet had
                // the chance to call Complete. In that case another Dispose call
                // can come in as the result of user cancellation, for instance.
                if (state == STATE_ACTIVE && !Process.HasExited) {
                    Process.Kill();
                }
            }
        }
    }
}