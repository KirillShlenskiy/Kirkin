using System;
using System.Collections.Generic;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Mutable command list builder.
    /// </summary>
    internal interface ICommandDefinitionContainer
    {
        /// <summary>
        /// Returns the collection of command definitions supported by this parser.
        /// </summary>
#if NET_40
        IEnumerable<CommandDefinition> Commands { get; }
#else
        IReadOnlyList<CommandDefinition> Commands { get; }
#endif
        /// <summary>
        /// Defines a command with the given name.
        /// </summary>
        void DefineCommand(string name, Action<CommandDefinition> configureAction);
    }
}