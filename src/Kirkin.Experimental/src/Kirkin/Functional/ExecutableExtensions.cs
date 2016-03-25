using System;

#if !NET_40
using System.Threading.Tasks;
#endif

namespace Kirkin.Functional
{
    /// <summary>
    /// Common extension methods for the <see cref="IExecutable" /> interface.
    /// </summary>
    internal static class ExecutableExtensions
    {
        /// <summary>
        /// Returns an executable which keeps retrying the execution
        /// allowing for the given number of exceptions.
        /// </summary>
        public static IExecutable RetryOnException(this IExecutable executable, int maxRetries)
        {
            return RetryOnException<Exception>(executable, maxRetries);
        }

        /// <summary>
        /// Returns an executable which keeps retrying the execution allowing
        /// for the given number of exceptions derived from the given type.
        /// </summary>
        public static IExecutable RetryOnException<TException>(this IExecutable executable, int maxRetries)
            where TException : Exception
        {
            if (executable == null) throw new ArgumentNullException("executable");
            if (maxRetries <= 0) throw new ArgumentOutOfRangeException("maxRetries");

            return Executable.Create(
                new Action(executable.Execute).Retry().OnException<TException>(maxRetries)
            );
        }

#if !NET_40
        /// <summary>
        /// Returns an executable which keeps retrying the execution
        /// allowing for the given number of exceptions.
        /// </summary>
        public static IAsyncExecutable RetryOnException(this IAsyncExecutable executable, int maxRetries)
        {
            return RetryOnException<Exception>(executable, maxRetries);
        }

        /// <summary>
        /// Returns an executable which keeps retrying the execution allowing
        /// for the given number of exceptions derived from the given type.
        /// </summary>
        public static IAsyncExecutable RetryOnException<TException>(this IAsyncExecutable executable, int maxRetries)
            where TException : Exception
        {
            if (executable == null) throw new ArgumentNullException("executable");
            if (maxRetries <= 0) throw new ArgumentOutOfRangeException("maxRetries");

            return Executable.CreateAsync(
                new Func<Task>(executable.ExecuteAsync).Retry().OnException<TException>(maxRetries)
            );
        }
#endif
    }
}