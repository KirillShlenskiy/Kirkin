#if !__MOBILE__ && !NETSTANDARD2_0

using System;
using System.Diagnostics;

using Kirkin.Windows;

namespace Kirkin.Diagnostics
{
    /// <summary>
    /// Utilities for creating associated processes.
    /// </summary>
    public static class ChildProcess
    {
        /// <summary>
        /// Starts a process whose lifetime is permanently linked to the
        /// lifetime of the current process. If the current process exits,
        /// the associated process is killed as well. Calling Dispose on
        /// the returned <see cref="Process"/> instance also kills it.
        /// </summary>
        /// <param name="startInfo">Process start params.</param>
        /// <param name="associated">True if the child process' lifetime has been tied to this process.</param>
        public static Process Start(ProcessStartInfo startInfo, out bool associated)
        {
            if (startInfo == null) throw new ArgumentNullException(nameof(startInfo));

            ChildProcessImpl childProcess = new ChildProcessImpl {
                StartInfo = startInfo
            };

            try
            {
                if (!childProcess.Start())
                {
                    childProcess.Dispose();

                    throw new InvalidOperationException("Unable to start process.");
                }
            }
            catch // Start may throw.
            {
                childProcess.Dispose();

                throw;
            }

            Job job = null;

            try
            {
                job = new Job();

                if (job.TryEnlistProcess(childProcess)) {
                    childProcess.Job = job; // Job != null indicates success.
                }
            }
            catch
            {
                // Job creation has failed. We don't want to throw as the
                // caller might want to do something with the Process object.
            }

            if (childProcess.Job == null)
            {
                job?.Dispose();

                associated = false;
            }
            else
            {
                associated = true;
            }

            return childProcess;
        }

        sealed class ChildProcessImpl : Process
        {
            internal Job Job;

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                Job?.Dispose();
            }
        }
    }
}

#endif