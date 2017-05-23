using System;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Command line argument whose value is only available once
    /// <see cref="CommandLineParser.Parse(string[])"/> has been called.
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
        /// Parsed argument value. Only available once <see cref="CommandLineParser.Parse(string[])"/> has been called.
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

        internal CommandArg(string name, string shortName, Func<T> resolver)
        {
            Name = name;
            ShortName = shortName;
            _resolver = resolver;
        }

        public override string ToString()
        {
            string name = (ShortName == null) ? Name : $"{ShortName}|{Name}";

            try
            {
                return $"{name}: {Value}";
            }
            catch (InvalidOperationException)
            {
                return $"{name}: value not available yet.";
            }
        }
    }
}