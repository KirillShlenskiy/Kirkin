using System;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Command line argument whose value is only available once
    /// <see cref="CommandLineParser.Parse(string[])"/> has been called.
    /// </summary>
    internal sealed class CommandArg<T> : ICommandArg
    {
        private readonly Func<string[], T> _valueConverter;

        /// <summary>
        /// Parameter name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Parameter short name (or null).
        /// </summary>
        public string ShortName { get; }

        internal CommandArg(string name, string shortName, Func<string[], T> valueConverter)
        {
            Name = name;
            ShortName = shortName;
            _valueConverter = valueConverter;
        }

        /// <summary>
        /// Parses the given arguments and converts them to an appropriate value.
        /// </summary>
        public T GetValue(string[] args)
        {
            return _valueConverter(args);
        }

        /// <summary>
        /// Parses the given arguments and converts them to an appropriate value.
        /// </summary>
        object ICommandArg.GetValue(string[] args)
        {
            return GetValue(args);
        }

        /// <summary>
        /// Returns the description of this instance.
        /// </summary>
        public override string ToString()
        {
            return $"{ShortName}|{Name}";
        }
    }
}