using System;
using System.Collections.Generic;

namespace Kirkin.Logging
{
    /// <summary>
    /// Logger entry formatter.
    /// </summary>
    public static class EntryFormatter
    {
        /// <summary>
        /// Builds a log entry pipeline where the entry passes through each
        /// of the formatters before hitting the final logEntry delegate.
        /// </summary>
        internal static Action<string> DecorateLogEntryDelegateWithFormatters(Action<string> logEntry, IEntryFormatter[] formatters)
        {
            for (int i = formatters.Length - 1; i >= 0; i--)
            {
                Action<string> tmp = logEntry;
                IEntryFormatter formatter = formatters[i];
                logEntry = e => formatter.LogEntry(e, tmp);
            }

            return logEntry;
        }

        /// <summary>
        /// Formatter which uses the given delegate as its
        /// <see cref="IEntryFormatter.LogEntry(string, Action{string})"/> implementation.
        /// </summary>
        internal static IEntryFormatter Create(Action<string, Action<string>> formatAction)
        {
            if (formatAction == null) throw new ArgumentNullException(nameof(formatAction));

            return new CustomEntryFormatter(formatAction);
        }

        sealed class CustomEntryFormatter : IEntryFormatter
        {
            private readonly Action<string, Action<string>> FormatAction;

            internal CustomEntryFormatter(Action<string, Action<string>> formatAction)
            {
                FormatAction = formatAction;
            }

            public void LogEntry(string entry, Action<string> logEntry)
            {
                FormatAction(entry, logEntry);
            }
        }

        /// <summary>
        /// Formatter which logs an additional entry
        /// specifying the number of seconds which passed since the
        /// last Log call, for the second and all subsequent Log calls.
        /// </summary>
        public static IEntryFormatter LogSecondsBetweenEntries(string format = "[Time elapsed: {0:0.000} s.]")
        {
            if (format == null) throw new ArgumentNullException(nameof(format));

            return new TimedEntryFormatter(format);
        }

        sealed class TimedEntryFormatter : IEntryFormatter
        {
            private readonly string TimeEntryFormat;

            // Environment.TickCount as at last Log call,
            // or zero if Log has not been called yet.
            private int TickCount;

            internal TimedEntryFormatter(string format)
            {
                TimeEntryFormat = format;
            }

            public void LogEntry(string entry, Action<string> logEntry)
            {
                int newTickCount = Environment.TickCount;

                if (TickCount != 0)
                {
                    int millisecondsElapsed = newTickCount - TickCount;
                    double secondsElapsed = (double)millisecondsElapsed / 1000;
                    string message = string.Format(TimeEntryFormat, secondsElapsed);

                    logEntry(message);
                }

                TickCount = newTickCount;

                logEntry(entry);
            }
        }

        /// <summary>
        /// Formatter which applies the given transformation to the entry.
        /// </summary>
        public static IEntryFormatter Transform(Func<string, string> transformation)
        {
            if (transformation == null) throw new ArgumentNullException(nameof(transformation));

            return new SelectEntryFormatter(transformation);
        }

        sealed class SelectEntryFormatter : IEntryFormatter
        {
            private readonly Func<string, string> Transformation;

            internal SelectEntryFormatter(Func<string, string> transformation)
            {
                Transformation = transformation;
            }

            public void LogEntry(string entry, Action<string> logEntry)
            {
                logEntry(Transformation(entry));
            }
        }

        /// <summary>
        /// Formatter which applies the given transformation to the entry.
        /// </summary>
        public static IEntryFormatter Transform(Func<string, IEnumerable<string>> transformation)
        {
            if (transformation == null) throw new ArgumentNullException(nameof(transformation));

            return new SelectManyEntryFormatter(transformation);
        }

        sealed class SelectManyEntryFormatter : IEntryFormatter
        {
            private readonly Func<string, IEnumerable<string>> Transformation;

            internal SelectManyEntryFormatter(Func<string, IEnumerable<string>> transformation)
            {
                Transformation = transformation;
            }

            public void LogEntry(string entry, Action<string> logEntry)
            {
                foreach (string transformedEntry in Transformation(entry)) {
                    logEntry(transformedEntry);
                }
            }
        }

        /// <summary>
        /// Formatter which splits multiline entries
        /// on line breaks and forwards each individual
        /// line to the inner logger.
        /// </summary>
        public static IEntryFormatter SplitMultilineEntries { get; } = new SplitLineEntryFormatter();

        sealed class SplitLineEntryFormatter : IEntryFormatter
        {
            public void LogEntry(string entry, Action<string> logEntry)
            {
                if (string.IsNullOrEmpty(entry))
                {
                    logEntry(entry);
                }
                else
                {
                    foreach (string l in entry.Split('\n'))
                    {
                        // Tidy up.
                        string line = l.Replace("\r", string.Empty);

                        logEntry(line);
                    }
                }
            }
        }

        /// <summary>
        /// Formatter which prepends each non-empty
        /// entry with a timestamp in the given format
        /// and forwards it to the inner logger.
        /// </summary>
        public static IEntryFormatter TimestampNonEmptyEntries(string format = "HH:mm:ss")
        {
            if (string.IsNullOrEmpty(format)) throw new ArgumentException("Timestamp format cannot be null or empty.");

            return new TimestampEntryFormatter(format);
        }

        sealed class TimestampEntryFormatter : IEntryFormatter
        {
            /// <summary>
            /// Timestamp format specified
            /// when this instance was created.
            /// </summary>
            private readonly string TimestampFormat;

            /// <summary>
            /// Creates a new instance of the logger.
            /// </summary>
            internal TimestampEntryFormatter(string timestampFormat)
            {
                TimestampFormat = timestampFormat;
            }

            /// <summary>
            /// Forwards the given entry to the inner logger.
            /// Prepends non-empty entries with a timestamp in
            /// the format specified when this instance was created.
            /// </summary>
            public void LogEntry(string entry, Action<string> logEntry)
            {
                if (!string.IsNullOrEmpty(entry)) {
                    entry = DateTime.Now.ToString(TimestampFormat) + " " + entry;
                }

                logEntry(entry);
            }
        }

        //#if !NET_40
        //        /// <summary>
        //        /// Returns a logger proxy which buffers messages
        //        /// and logs them when there is a large enough break.
        //        /// </summary>
        //        /// <param name="logger">Logger which will be proxied.</param>
        //        /// <param name="bufferDuration">Specifies time period which must pass between the last call to Log, and buffer flush.</param>
        //        /// <param name="captureContext">
        //        /// If true, buffer flush will occur on the synchronization context captured at the beginning ot the Log call. Otherwise thread pool will be used.
        //        /// </param>
        //        public static Logger WithAsyncBuffering(this Logger logger, TimeSpan bufferDuration, bool captureContext)
        //        {
        //            return new AsyncLoggerWrapper(logger, bufferDuration, captureContext);
        //        }

        //        sealed class AsyncLoggerWrapper : LoggerWrapper
        //        {
        //            private readonly List<string> EntryBuffer = new List<string>();
        //            private readonly DelayTaskFactory DelayTaskFactory = new DelayTaskFactory(DelayTaskCancellationMode.SetTaskResultToFalse);

        //            public TimeSpan BufferDuration { get; }
        //            public bool CaptureContext { get; }

        //            internal AsyncLoggerWrapper(Logger logger, TimeSpan bufferDuration, bool captureContext)
        //                : base(logger)
        //            {
        //                CaptureContext = captureContext;
        //            }

        //            protected override async void LogEntry(string entry)
        //            {
        //                Task<bool> delayTask = DelayTaskFactory.Restart(BufferDuration);

        //                lock (EntryBuffer) {
        //                    EntryBuffer.Add(entry);
        //                }

        //                bool ranToCompletion = await delayTask.ConfigureAwait(CaptureContext);

        //                if (ranToCompletion) {
        //                    LogBufferedEntries();
        //                }
        //            }

        //            private void LogBufferedEntries()
        //            {
        //                string[] buffer;

        //                // Keep the lock just long enough to flush the buffer
        //                // so that subsequent calls to Log don't have to block.
        //                lock (EntryBuffer)
        //                {
        //                    if (EntryBuffer.Count == 0) {
        //                        return;
        //                    }

        //                    // Buffer flush.
        //                    buffer = EntryBuffer.ToArray();
        //                    EntryBuffer.Clear();

        //                    System.Diagnostics.Debug.Assert(EntryBuffer.Count == 0, "EntryBuffer not fully drained prior to lock release.");
        //                }

        //                foreach (string entry in buffer) {
        //                    base.LogEntry(entry);
        //                }
        //            }
        //        }
        //#endif
    }
}