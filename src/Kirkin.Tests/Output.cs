using System;
using Kirkin.Logging;

namespace Kirkin.Tests
{
    internal static class Output
    {
        private static readonly Logger Logger = new LoggerBuilder(WriteLine)
            .AddFormatter(EntryFormatter.TimestampNonEmptyEntries())
            .BuildLogger();

        public static void Log(string message)
        {
            Logger.Log(message);
        }

        public static void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}