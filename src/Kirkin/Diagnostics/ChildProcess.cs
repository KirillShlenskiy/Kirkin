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
        public static Process Start(ProcessStartInfo startInfo)
        {
            if (startInfo == null) throw new ArgumentNullException(nameof(startInfo));

            Job job = null;
            ChildProcessImpl childProcess = new ChildProcessImpl();

            try
            {
                childProcess.StartInfo = startInfo;

                if (childProcess.Start())
                {
                    job = new Job();

                    job.EnlistProcess(childProcess);

                    childProcess.Job = job; // Job != null indicates success.

                    return childProcess;
                }
            }
            finally
            {
                if (childProcess.Job == null)
                {
                    // Child process may continue running after Dispose if adding it to a job failed.
                    childProcess.Dispose();
                    job?.Dispose();
                }
            }

            return null;
        }

        sealed class ChildProcessImpl : Process
        {
            internal Job Job;

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                Job.Dispose();
            }
        }
    }
}

#endif