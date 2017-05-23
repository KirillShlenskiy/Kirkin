using System;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Parsed command line argument whose value is only available
    /// once <see cref="ICommand.Execute"/> has been called.
    /// </summary>
    public sealed class Arg<T>
    {
        private readonly Func<T> _resolver;

        /// <summary>
        /// Parsed argument value. Only available once <see cref="ICommand.Execute"/> has been called.
        /// </summary>
        public T Value
        {
            get
            {
                return _resolver();
            }
        }

        internal Arg(Func<T> resolver)
        {
            _resolver = resolver;
        }
    }
}