﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Kirkin
{
    /// <summary>
    /// <see cref="Process"/> wrapper that facilitates console app interop.
    /// </summary>
    public sealed class ConsoleRunner
    {
        private Process _process;

        /// <summary>
        /// Executable name/path specified when this instance was created.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Arguments specified when this instance was created.
        /// </summary>
        public string Arguments { get; }

        /// <summary>
        /// <see cref="System.Diagnostics.Process"/> created by the Run call.
        /// </summary>
        public Process Process
        {
            get
            {
                return _process;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="ConsoleRunner"/> with the given executable name and args.
        /// </summary>
        public ConsoleRunner(string fileName, string args = null)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentException("File name must be specified.");

            FileName = fileName;
            Arguments = args;
        }

        /// <summary>
        /// Raised when output is received from the console app.
        /// Gets raised on a background thread.
        /// </summary>
        public event Action<string> Output;

        /// <summary>
        /// Runs the console app process.
        /// </summary>
        public void Run()
        {
            Task t = RunImpl(false, default(CancellationToken));

            if (!t.IsCompleted) {
                throw new InvalidOperationException("Expecting RunImpl to have completed synchronously.");
            }
        }

        /// <summary>
        /// Runs the console app process.
        /// </summary>
        public Task RunAsync()
        {
            return RunImpl(true, default(CancellationToken));
        }

        /// <summary>
        /// Runs the console app process.
        /// </summary>
        public Task RunAsync(CancellationToken cancellationToken)
        {
            return RunImpl(true, cancellationToken);
        }

        // Implementation.
        private async Task RunImpl(bool async, CancellationToken cancellationToken)
        {
            if (_process != null) {
                throw new InvalidOperationException("Ypu should not call Run multiple times. The process has already been created.");
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo(FileName) {
                Arguments = Arguments,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            EventHandler processExitHandler = delegate
            {
                Process p = Interlocked.Exchange(ref _process, null);

                if (p != null) {
                    p.Kill();
                };
            };

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(() => processExitHandler(null, null), useSynchronizationContext: false);
                cancellationToken.ThrowIfCancellationRequested();
            }
            
            // Ensure the child process is killed if the parent exits.
            AppDomain.CurrentDomain.ProcessExit += processExitHandler;

            try
            {
                _process = Process.Start(processStartInfo);

                _process.EnableRaisingEvents = true;

                _process.OutputDataReceived += (s, e) =>
                {
                    string line = e.Data;

                    Output?.Invoke(line);
                };

                _process.BeginOutputReadLine();

                if (async)
                {
                    TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

                    _process.Exited += (s, e) => tcs.SetResult(null);

                    await tcs.Task.ConfigureAwait(false); // Long-running operation.
                }
                else
                {
                    _process.WaitForExit();
                }

                // By now the child process could have been killed and nulled out by the
                // OutputDataReceived event handler. Handle this scenario gracefully.
                // No need for memory barrier - it is inserted by the await.
                Process p = _process;

                if (p == null)
                {
                    if (!cancellationToken.IsCancellationRequested) {
                        throw new InvalidOperationException("Unexpected runner state. Expecting token to be marked as canceled.");
                    }

                    throw new OperationCanceledException("Child process forcibly terminated.", cancellationToken);
                }
                else
                {
                    int result = p.ExitCode;

                    if (result != 0) {
                        throw new Win32Exception(result, "Non-zero exit code.");
                    }
                }
            }
            finally
            {
                AppDomain.CurrentDomain.ProcessExit -= processExitHandler;
                Process p = Interlocked.Exchange(ref _process, null);

                if (p != null) {
                    p.Close();
                }
            }
        }
    }
}