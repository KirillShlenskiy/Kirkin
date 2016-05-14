using System;
using System.IO;

namespace Kirkin.Logging
{
    /// <summary>
    /// Returns a logger which which writes messages to the file at the given path.
    /// Creates the file if it does not exist whenever an entry is logged.
    /// </summary>
    public sealed class FileLogger : Logger
    {
        /// <summary>
        /// Log file path specified when this instance was created.
        /// </summary>
        public string LogFilePath { get; }

        /// <summary>
        /// Creates a new instance of <see cref="FileLogger"/>
        /// which logs entries to a file at the given path.
        /// </summary>
        public FileLogger(string logFilePath)
        {
            if (string.IsNullOrEmpty(logFilePath)) throw new ArgumentException("Log file path cannot be null or empty.");

            LogFilePath = logFilePath;
        }

        /// <summary>
        /// Writes the given entry to the file. Creates the file if necessary.
        /// </summary>
        protected override void LogEntry(string entry)
        {
            string directoryPath = Path.GetDirectoryName(LogFilePath);

            if (!Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }

            File.AppendAllText(LogFilePath, entry + Environment.NewLine);
        }
    }
}