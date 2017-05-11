using System;

namespace Kirkin
{
    /// <summary>
    /// Exception raised when the child process terminates with a non-zero exit code.
    /// </summary>
    [Serializable]
    public sealed class ConsoleRunnerException : Exception
    {
        /// <summary>
        /// Child process exit code.
        /// </summary>
        public int ExitCode { get; }

        internal ConsoleRunnerException(int exitCode, string message)
            : base(message)
        {
            ExitCode = exitCode;
        }
    }
}