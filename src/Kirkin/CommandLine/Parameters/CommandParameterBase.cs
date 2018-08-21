using System.Collections.Generic;

namespace Kirkin.CommandLine.Parameters
{
    /// <summary>
    /// Command parameter definition.
    /// </summary>
    internal abstract class CommandParameterBase<T>
        : CommandParameter
    {
        /// <summary>
        /// Creates a new <see cref="CommandParameterBase{T}"/> instance.
        /// </summary>
        internal CommandParameterBase(string name, string shortName, bool isPositionalParameter, string help)
            : base(name, shortName, isPositionalParameter, help)
        {
        }

        /// <summary>
        /// Parses the given arguments and converts them to an appropriate value.
        /// </summary>
        public abstract T ParseArgsImpl(List<string> args);

        /// <summary>
        /// Returns the default value to be used when the parameter, switch or option is omitted.
        /// </summary>
        public virtual T GetDefaultValueImpl()
        {
            return default;
        }

        /// <summary>
        /// Parses the given arguments and converts them to an appropriate value.
        /// </summary>
        internal override object ParseArgs(List<string> args)
        {
            return ParseArgsImpl(args);
        }

        /// <summary>
        /// Returns the default value to be used when the parameter, switch or option is omitted.
        /// </summary>
        internal override object GetDefaultValue()
        {
            return GetDefaultValueImpl();
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