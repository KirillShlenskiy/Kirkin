using System;

using Kirkin.Functional;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Console formatting utilities.
    /// </summary>
    public static class ConsoleFormatter
    {
        /// <summary>
        /// Sets the foreground colour of the console to the specified value
        /// and reliably resets it once the returned object id disposed.
        /// </summary>
        public static IDisposable ForegroundColorScope(ConsoleColor foregroundColor)
        {
            ConsoleColor originalColor = Console.ForegroundColor;

            Console.ForegroundColor = foregroundColor;

            return Disposable.Create(() => Console.ForegroundColor = originalColor);
        }
    }
}