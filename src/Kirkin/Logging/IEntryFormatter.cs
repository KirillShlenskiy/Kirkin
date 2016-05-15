using System;

namespace Kirkin.Logging
{
    /// <summary>
    /// Logger entry formatter.
    /// </summary>
    public interface IEntryFormatter
    {
        /// <summary>
        /// Formats the given entry and logs it using the given delegate.
        /// </summary>
        void LogEntry(string entry, Action<string> logEntry);
    }
}