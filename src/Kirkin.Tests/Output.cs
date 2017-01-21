using System;

using Kirkin.Logging;

namespace Kirkin.Tests
{
    /// <summary>
    /// Output helper useful when converting xUnit projects to NUnit.
    /// </summary>
    internal static class Output
    {
        private static readonly Logger Logger = new LoggerBuilder(WriteLine)
            .AddFormatter(EntryFormatter.TimestampNonEmptyEntries())
            .BuildLogger();

        /// <summary>
        /// Logs the given message using a <see cref="Kirkin.Logging.Logger"/>
        /// instance which prefixes messages with timestamps.
        /// </summary>
        public static void Log(string message)
        {
            Logger.Log(message);
        }
        
        /// <summary>
        /// Writes the given message directly to the console.
        /// </summary>
        public static void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}