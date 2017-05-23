using System;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Parsed command line argument whose value is only available
    /// once <see cref="ICommand.Execute"/> has been called.
    /// </summary>
    public sealed class CommandArg<T> : ICommandArg
    {
        private readonly Func<T> _resolver;

        /// <summary>
        /// Parameter name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Parameter short name (or null).
        /// </summary>
        public string ShortName { get; }

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

        object ICommandArg.Value
        {
            get
            {
                return Value;
            }
        }

        internal CommandArg(Func<T> resolver)
        {
            _resolver = resolver;
        }
    }
}