using System;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Command parameter definition.
    /// </summary>
    internal sealed class CommandParameter<T> : ICommandParameter
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

        /// <summary>
        /// Creates a new <see cref="CommandParameter{T}"/> instance.
        /// </summary>
        internal CommandParameter(string name, string shortName, Func<string[], T> valueConverter)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Parameter name cannot be empty.");
            if (valueConverter == null) throw new ArgumentNullException(nameof(valueConverter));

            Name = name;
            ShortName = shortName;
            _valueConverter = valueConverter;
        }

        /// <summary>
        /// Parses the given arguments and converts them to an appropriate value.
        /// </summary>
        public T ParseArgs(string[] args)
        {
            return _valueConverter(args);
        }

        /// <summary>
        /// Parses the given arguments and converts them to an appropriate value.
        /// </summary>
        object ICommandParameter.ParseArgs(string[] args)
        {
            return ParseArgs(args);
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