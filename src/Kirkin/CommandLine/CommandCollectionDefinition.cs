using System;
using System.Collections.Generic;

using Kirkin.CommandLine.Help;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Container for multiple logically grouped commands.
    /// </summary>
    public sealed class CommandCollectionDefinition : CommandDefinition, ICommandDefinitionContainer
    {
        private readonly CommandLineParser Parser;

        /// <summary>
        /// Returns the collection of command definitions supported by this parser.
        /// </summary>
#if NET_40
        public IEnumerable<CommandDefinition> Commands => Parser.Commands;
#else
        public IReadOnlyList<CommandDefinition> Commands => Parser.Commands;
#endif
        internal IEqualityComparer<string> StringEqualityComparer => Parser.StringEqualityComparer;

        internal CommandCollectionDefinition(string name, CommandDefinition parent, bool caseInsensitive)
            : base(name, parent)
        {
            Parser = new CommandLineParser {
                CaseInsensitive = caseInsensitive,
                Parent = this
            };
        }

        /// <summary>
        /// Defines a command with the given name.
        /// </summary>
        public void DefineCommand(string name, Action<IndividualCommandDefinition> configureAction)
        {
            Parser.DefineCommand(name, configureAction);
        }

        /// <summary>
        /// Defines a group of commands with the given name.
        /// </summary>
        public void DefineCommandCollection(string name, Action<CommandCollectionDefinition> configureAction)
        {
            Parser.DefineCommandCollection(name, configureAction);
        }

        /// <summary>
        /// Parses the command line args and returns the configured, ready-to-execute command.
        /// </summary>
        internal override ICommand Parse(string[] args)
        {
            if (args.Length == 0 || (args.Length == 1 && CommandSyntax.IsHelpSwitch(args[0], StringEqualityComparer))) {
                return new CommandCollectionDefinitionHelpCommand(this);
            }

            return Parser.Parse(args);
        }

        private protected override IHelpCommand CreateHelpCommand()
        {
            return new CommandCollectionDefinitionHelpCommand(this);
        }

        /// <summary>
        /// Returns the string description of this command group.
        /// </summary>
        public override string ToString()
        {
            return $"{Name} <command>";
        }
    }
}