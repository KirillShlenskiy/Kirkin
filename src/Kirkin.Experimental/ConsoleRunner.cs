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
        /// <summary>
        /// Executable name/path specified when this instance was created.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Arguments specified when this instance was created.
        /// </summary>
        public string Arguments { get; }

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
        public Task RunAsync()
        {
            return RunAsync(default(CancellationToken));
        }

        /// <summary>
        /// Runs the console app process.
        /// </summary>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo(FileName) {
                Arguments = Arguments,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            Process process = null;

            EventHandler processExitHandler = delegate
            {
                Process p = Interlocked.Exchange(ref process, null);

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
                process = Process.Start(processStartInfo);

                process.EnableRaisingEvents = true;

                process.OutputDataReceived += (s, e) =>
                {
                    string line = e.Data;

                    Output?.Invoke(line);
                };

                process.BeginOutputReadLine();

                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

                process.Exited += (s, e) => tcs.SetResult(null);

                await tcs.Task.ConfigureAwait(false); // Long-running operation.

                // By now the child process could have been killed and nulled out by the
                // OutputDataReceived event handler. Handle this scenario gracefully.
                // No need for memory barrier - it is inserted by the await.
                Process p = process;

                if (p == null)
                {
                    // Ultimately gets converted to a TaskCanceledException.
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
                Process p = Interlocked.Exchange(ref process, null);

                if (p != null) {
                    p.Close();
                }
            }
        }
    }
}