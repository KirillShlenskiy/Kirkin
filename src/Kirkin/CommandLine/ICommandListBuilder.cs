using System;
using System.Collections.Generic;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Mutable command list builder.
    /// </summary>
    public interface ICommandListBuilder
    {
        /// <summary>
        /// Returns the collection of command definitions supported by this parser.
        /// </summary>
#if NET_40
        IEnumerable<ICommandDefinition> CommandDefinitions { get; }
#else
        IReadOnlyList<ICommandDefinition> CommandDefinitions { get; }
#endif
        /// <summary>
        /// Defines a command with the given name.
        /// </summary>
        void DefineCommand(string name, Action<CommandDefinition> configureAction);

        /// <summary>
        /// Defines a group of commands with the given name.
        /// </summary>
        void DefineCommandGroup(string name, Action<CommandGroupDefinition> configureAction);
    }
}