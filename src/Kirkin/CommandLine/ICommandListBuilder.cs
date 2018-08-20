using System;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Mutable command list builder.
    /// </summary>
    public interface ICommandListBuilder
    {
        /// <summary>
        /// Defines a command with the given name.
        /// </summary>
        void DefineCommand(string name, Action<CommandDefinition> configureAction);

        /// <summary>
        /// Defines a group of commands with the given name.
        /// </summary>
        void DefineCommandGroup(string name, Action<ICommandListBuilder> configureAction);
    }
}