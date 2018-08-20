using System;

using Kirkin.CommandLine.Commands;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Container for multiple logically grouped commands.
    /// </summary>
    public sealed class CommandGroupDefinition : CommandDefinitionBase, ICommandListBuilder
    {
        private readonly CommandLineParser Parser;

        internal CommandGroupDefinition(string name, bool caseInsensitive)
            : base(name)
        {
            Parser = new CommandLineParser { CaseInsensitive = caseInsensitive };
        }

        /// <summary>
        /// Defines a command with the given name.
        /// </summary>
        public void DefineCommand(string name, Action<CommandDefinition> configureAction)
        {
            Parser.DefineCommand(name, configureAction);
        }

        /// <summary>
        /// Defines a group of commands with the given name.
        /// </summary>
        public void DefineCommandGroup(string name, Action<CommandGroupDefinition> configureAction)
        {
            Parser.DefineCommandGroup(name, configureAction);
        }

        /// <summary>
        /// Parses the command line args and returns the configured, ready-to-execute command.
        /// </summary>
        public override ICommand Parse(string[] args)
        {
            return Parser.Parse(args);
        }

        private protected override IHelpCommand CreateHelpCommand()
        {
            return new CommandGroupHelpCommand(this);
        }
    }
}