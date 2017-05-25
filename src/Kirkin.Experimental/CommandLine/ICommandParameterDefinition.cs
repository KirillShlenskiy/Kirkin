using System.Collections.Generic;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Command parameter definition.
    /// </summary>
    internal interface ICommandParameterDefinition : ICommandParameter
    {
        /// <summary>
        /// Parses the given arguments and converts them to an appropriate value.
        /// </summary>
        object ParseArgs(List<string> args);

        /// <summary>
        /// Returns the default value to be used when the parameter, switch or option is omitted.
        /// </summary>
        object GetDefaultValue();
    }
}