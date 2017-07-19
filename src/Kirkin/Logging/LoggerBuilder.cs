using System;
using System.Collections.Generic;

namespace Kirkin.Logging
{
    /// <summary>
    /// Type responsible for building <see cref="Logger"/> instances.
    /// </summary>
    public sealed class LoggerBuilder
    {
        // Logger instance or log delegate (Action<string>).
        private readonly object LoggerObj;
        private readonly List<IEntryFormatter> _formatters = new List<IEntryFormatter>();

        /// <summary>
        /// Entry formatters that will be applied to the newly created logger.
        /// </summary>
        public ICollection<IEntryFormatter> Formatters
        {
            get
            {
                return _formatters;
            }
        }

        /// <summary>
        /// Creates a new <see cref="LoggerBuilder"/> instance with
        /// the intent of adding a logging target at a later point.
        /// </summary>
        public LoggerBuilder()
            : this(Logger.Null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="LoggerBuilder"/> instance which
        /// will produce loggers that perform the given logging action.
        /// </summary>
        public LoggerBuilder(Action<string> logAction)
        {
            if (logAction == null) throw new ArgumentNullException(nameof(logAction));

            LoggerObj = logAction;
        }

        /// <summary>
        /// Creates a new <see cref="LoggerBuilder"/> instance which
        /// will produce loggers that write to the given final logger.
        /// </summary>
        public LoggerBuilder(Logger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            LoggerObj = logger;
        }

        /// <summary>
        /// Creates an configures a <see cref="Logger"/> instance.
        /// </summary>
        public Logger BuildLogger()
        {
            Logger logger = LoggerObj as Logger
                ?? Logger.Create((Action<string>)LoggerObj);

            return (_formatters.Count == 0)
                ? logger
                : Logger.Create(EntryFormatter.DecorateLogEntryDelegateWithFormatters(logger.Log, _formatters));
        }
    }
}