using System;
using System.Diagnostics.CodeAnalysis;

namespace Kirkin.Logging
{
    /// <summary>
    /// Entry block which ensures that the logger logs an empty entry when
    /// the block is created, and another one when the block is disposed.
    /// </summary>
    public struct LogEntryBlock : IDisposable // Solely for the sake of the "using" statement.
    {
        #region Static methods

        /// <summary>
        /// Logs the given entry as a block surrounded by start and end tokens.
        /// </summary>
        public static void Log(Logger logger, string entry)
        {
            EnsureEmptyEntry(logger);

            // If this fails, there is no need to write
            // the closing entry (hence no try-finally).
            logger.Log(entry);

            EnsureEmptyEntry(logger);
        }

        /// <summary>
        /// Writes an empty entry to the log if appropriate.
        /// </summary>
        static void EnsureEmptyEntry(Logger logger)
        {
            if (!string.IsNullOrEmpty(logger.LastEntry)) {
                logger.Log("");
            }
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Logger which created this block.
        /// </summary>
        private readonly Logger Logger;

        /// <summary>
        /// Creates a new entry block surrounded by empty entries.
        /// Writes the starting entry immediately.
        /// </summary>
        public LogEntryBlock(Logger logger)
        {
            Logger = logger;

            EnsureEmptyEntry(Logger);
        }

        /// <summary>
        /// Closes the entry block by ensuring that an empty entry is logged.
        /// </summary>
        public void Complete()
        {
            EnsureEmptyEntry(Logger);
        }

        /// <summary>
        /// Closes the entry block by ensuring that an empty entry is logged.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
        void IDisposable.Dispose() // Solely for the sake of the "using" statement.
        {
            Complete();
        }

        #endregion
    }
}