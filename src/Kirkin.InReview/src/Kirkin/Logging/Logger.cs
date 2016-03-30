using System;

namespace Kirkin.Logging
{
    /// <summary>
    /// Base class and factory for common logger implementations.
    /// </summary>
    public abstract class Logger
    {
        #region Implementation

        /// <summary>
        /// Gets the last entry processed by this logger.
        /// </summary>
        protected internal string LastEntry { get; private set; }

        /// <summary>
        /// Logs the given formatted entry.
        /// </summary>
        public void Log(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }

        /// <summary>
        /// Logs the given entry.
        /// </summary>
        public void Log(string entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            LogEntry(entry);

            LastEntry = entry;
        }

        /// <summary>
        /// When overridden in a derived class, provides logging implementation.
        /// </summary>
        /// <remarks>
        /// This had to be split into a separate method to allow
        /// async implementations to be mixed with standard Log logic.
        /// </remarks>
        protected abstract void LogEntry(string entry);

        #endregion

        #region Fluent API

        /// <summary>
        /// Returns a logger wrapper which applies the given formatters
        /// to each logged entry and logs the result to this logger.
        /// </summary>
        public Logger WithFormatters(params IEntryFormatter[] formatters)
        {
            if (formatters == null) throw new ArgumentNullException(nameof(formatters));
            if (formatters.Length == 0) return this;

            // No need for defensive copy. The array will be discarded.
            return new CustomLogger(
                entryAction: EntryFormatter.DecorateLogEntryDelegateWithFormatters(Log, formatters)
            );
        }

        ///// <summary>
        ///// Returns a logger wrapper which applies the given formatters
        ///// to each logged entry and logs the result to this logger.
        ///// </summary>
        //public Logger WithFormatters(EntryFormatter formatter)
        //{
        //    if (formatter == null) throw new ArgumentNullException(nameof(formatter));

        //    return new SingleFormatterLogger(this, formatter);
        //}

        //sealed class SingleFormatterLogger : Logger
        //{
        //    private readonly Action<string> LogAction;
        //    private readonly EntryFormatter Formatter;

        //    internal SingleFormatterLogger(Logger inner, EntryFormatter formatter)
        //    {
        //        LogAction = inner.LogEntry;
        //        Formatter = formatter;
        //    }

        //    protected override void LogEntry(string entry)
        //    {
        //        Formatter.LogEntry(entry, LogAction);
        //    }
        //}

        #endregion

        /// <summary>
        /// Wraps multiple loggers into a single immutable instance.
        /// </summary>
        public static Logger Combine(params Logger[] loggers)
        {
            if (loggers == null) throw new ArgumentNullException(nameof(loggers));
            if (loggers.Length == 0) throw new ArgumentException(nameof(loggers));

            Logger[] defensiveCopy = new Logger[loggers.Length];
            Array.Copy(loggers, 0, defensiveCopy, 0, loggers.Length);
            return new MultLoggerWrapper(defensiveCopy);
        }

        sealed class MultLoggerWrapper : Logger
        {
            private readonly Logger[] InnerLoggers;

            public MultLoggerWrapper(Logger[] loggers)
            {
                InnerLoggers = loggers;
            }

            protected override void LogEntry(string entry)
            {
                foreach (Logger logger in InnerLoggers) {
                    logger.Log(entry);
                }
            }
        }

        /// <summary>
        /// Creates a new logger which uses a custom action to process entries.
        /// </summary>
        public static Logger Create(Action<string> logAction)
        {
            if (logAction == null) throw new ArgumentNullException(nameof(logAction));

            return new CustomLogger(logAction);
        }

        sealed class CustomLogger : Logger
        {
            private readonly Action<string> EntryAction;

            public CustomLogger(Action<string> entryAction)
            {
                EntryAction = entryAction;
            }

            protected override void LogEntry(string entry)
            {
                EntryAction(entry);
            }
        }

        /// <summary>
        /// Returns a logger instance which logs to debug output.
        /// </summary>
        public static Logger Debug { get; } = new DebugLogger();

        sealed class DebugLogger : Logger
        {
            protected override void LogEntry(string entry)
            {
                System.Diagnostics.Debug.Print(entry);
            }
        }

        /// <summary>
        /// No-op logger.
        /// </summary>
        public static Logger Null { get; } = new NullLogger();

        /// <summary>
        /// No-op logger implementation.
        /// </summary>
        sealed class NullLogger : Logger
        {
            protected override void LogEntry(string entry)
            {
            }
        }

        /// <summary>
        /// Returns a logger instance which protects its Log calls with a lock.
        /// </summary>
        public static Logger Synchronised(Logger logger)
        {
            return Synchronised(logger, new object());
        }

        /// <summary>
        /// Returns a logger instance which protects its Log calls with a lock on the given object.
        /// </summary>
        public static Logger Synchronised(Logger logger, object lockObj)
        {
            return new SynchronisedLogger(logger, lockObj);
        }

        /// <summary>
        /// Logger implementation which protects its Log calls with a lock.
        /// </summary>
        sealed class SynchronisedLogger : Logger
        {
            private readonly Logger Inner;
            private readonly object LockObj;

            public SynchronisedLogger(Logger inner, object lockObj)
            {
                Inner = inner;
                LockObj = lockObj;
            }

            protected override void LogEntry(string entry)
            {
                lock (LockObj) {
                    Inner.Log(entry);
                }
            }
        }
    }
}