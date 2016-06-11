using System;
using System.Collections.Generic;

namespace Kirkin.Logging
{
    /// <summary>
    /// Fluent <see cref="LoggerBuilder"/> extension methods.
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
    }
}