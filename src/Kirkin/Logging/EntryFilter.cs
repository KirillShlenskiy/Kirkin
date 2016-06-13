using System;
using System.Collections.Generic;

namespace Kirkin.Logging
{
    /// <summary>
    /// Special type of <see cref="IEntryFormatter"/> which treats logger input as <see cref="IEnumerable{String}"/>.
    /// Allows the consumers to use LINQ to process the stream of log entries.
    /// </summary>
    internal sealed class EntryFilter : IEntryFormatter
    {
        private readonly Func<IEnumerable<string>, IEnumerable<string>> Selector;

        /// <summary>
        /// Creates a new <see cref="EntryFilter"/> instance with the given selector function.
        /// </summary>
        public EntryFilter(Func<IEnumerable<string>, IEnumerable<string>> selector)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            Selector = selector;
        }

        /// <summary>
        /// Filters the input and performs the given action on the result.
        /// </summary>
        public void LogEntry(string entry, Action<string> logEntry)
        {
            string[] entries = { entry };

            foreach (string filteredEntry in Selector(entries)) {
                logEntry(filteredEntry);
            }
        }
    }
}