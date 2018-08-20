using System;
using System.Collections.Generic;

namespace Kirkin.CommandLine.Parameters
{
    /// <summary>
    /// Command parameter definition.
    /// </summary>
    internal abstract class CommandParameterBase<T>
        : ICommandParameterDefinition
    {
        /// <summary>
        /// Parameter name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Parameter short name (or null).
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// Returns true if this parameter/option supports multiple input values.
        /// </summary>
        public abstract bool SupportsMultipleValues { get; }

        /// <summary>
        /// Specifies whether this parameter can be resolved by its position in addition to than name.
        /// </summary>
        public bool IsPositionalParameter { get; }

        /// <summary>
        /// Human-readable parameter description.
        /// </summary>
        public string Help { get; }

        /// <summary>
        /// Creates a new <see cref="CommandParameterBase{T}"/> instance.
        /// </summary>
        internal CommandParameterBase(string name, string shortName, bool isPositionalParameter, string help)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Parameter name cannot be empty.");

            CommandSyntax.EnsureNotAReservedKeyword(name);

            if (shortName != null) {
                CommandSyntax.EnsureNotAReservedKeyword(shortName);
            }

            Name = name;
            ShortName = shortName;
            IsPositionalParameter = isPositionalParameter;
            Help = help;
        }

        /// <summary>
        /// Parses the given arguments and converts them to an appropriate value.
        /// </summary>
        public abstract T ParseArgs(List<string> args);

        /// <summary>
        /// Returns the default value to be used when the parameter, switch or option is omitted.
        /// </summary>
        public virtual T GetDefaultValue()
        {
            return default(T);
        }

        /// <summary>
        /// Parses the given arguments and converts them to an appropriate value.
        /// </summary>
        object ICommandParameterDefinition.ParseArgs(List<string> args)
        {
            return ParseArgs(args);
        }

        /// <summary>
        /// Returns the default value to be used when the parameter, switch or option is omitted.
        /// </summary>
        object ICommandParameterDefinition.GetDefaultValue()
        {
            return GetDefaultValue();
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