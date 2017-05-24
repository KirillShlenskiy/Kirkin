using System;
using System.Collections.Generic;

namespace Kirkin.CommandLine.Parameters
{
    /// <summary>
    /// Command parameter definition.
    /// </summary>
    internal abstract class CommandParameterBase<T>
        : ICommandParameter
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
        /// Creates a new <see cref="CommandParameter{T}"/> instance.
        /// </summary>
        internal CommandParameterBase(string name, string shortName)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Parameter name cannot be empty.");

            Name = name;
            ShortName = shortName;
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
        object ICommandParameter.ParseArgs(List<string> args)
        {
            return ParseArgs(args);
        }

        /// <summary>
        /// Returns the default value to be used when the parameter, switch or option is omitted.
        /// </summary>
        object ICommandParameter.GetDefaultValue()
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