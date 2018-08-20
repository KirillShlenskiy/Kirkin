using System;
using System.Collections.Generic;

using Kirkin.CommandLine.Commands;

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
        IEnumerable<CommandDefinition> CommandDefinitions { get; }
#else
        IReadOnlyList<CommandDefinition> CommandDefinitions { get; }
#endif
        /// <summary>
        /// Defines a command with the given name.
        /// </summary>
        void DefineCommand(string name, Action<IndividualCommandDefinition> configureAction);

        /// <summary>
        /// Defines a group of commands with the given name.
        /// </summary>
        void DefineCommandGroup(string name, Action<GroupCommandDefinition> configureAction);
    }
}