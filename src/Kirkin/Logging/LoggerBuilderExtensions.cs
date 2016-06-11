using System;
using System.Collections.Generic;

namespace Kirkin.Logging
{
    /// <summary>
    /// Fluent <see cref="LoggerBuilder"/> extension methods.
    /// Facilitates building complex logging chains.
    /// </summary>
    public static class LoggerBuilderExtensions
    {
        /// <summary>
        /// Mutates the given builder's <see cref="LoggerBuilder.Formatters"/>
        /// collection by adding a log entry filter with the given parameters
        /// to it, and returns the mutated <see cref="LoggerBuilder"/> instance.
        /// </summary>
        public static LoggerBuilder AddFilter(this LoggerBuilder builder, Func<IEnumerable<string>, IEnumerable<string>> entryFilter)
        {
            builder.Formatters.Add(new EntryFilter(entryFilter));

            return builder;
        }

        /// <summary>
        /// Mutates the given builder's <see cref="LoggerBuilder.Formatters"/>
        /// collection by adding a log entry formatter to it, and
        /// returns the mutated <see cref="LoggerBuilder"/> instance.
        /// </summary>
        public static LoggerBuilder AddFormatter(this LoggerBuilder builder, IEntryFormatter entryFormatter)
        {
            builder.Formatters.Add(entryFormatter);

            return builder;
        }

        /// <summary>
        /// Mutates the given builder by adding a logger which will process the entry when
        /// the built logger insance's <see cref="Logger.Log(string)"/> mehod is called. 
        /// </summary>
        public static LoggerBuilder AddLogger(this LoggerBuilder builder, Logger logger)
        {
            IEntryFormatter formatter = EntryFormatter.Transform(entry =>
            {
                logger.Log(entry);

                return entry;
            });

            builder.Formatters.Add(formatter);

            return builder;
        }

        /// <summary>
        /// Mutates the given builder by adding a logging action to be invoked when
        /// the built logger insance's <see cref="Logger.Log(string)"/> mehod is called. 
        /// </summary>
        public static LoggerBuilder AddLogAction(this LoggerBuilder builder, Action<string> logAction)
        {
            IEntryFormatter formatter = EntryFormatter.Transform(entry =>
            {
                logAction(entry);

                return entry;
            });

            builder.Formatters.Add(formatter);

            return builder;
        }
    }
}