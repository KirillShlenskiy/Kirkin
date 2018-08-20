﻿using System;
using System.Collections.Generic;
using System.Linq;

using Kirkin.CommandLine.Commands;
using Kirkin.CommandLine.Commands.Help;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Command line argument parser.
    /// </summary>
    public sealed class CommandLineParser : ICommandDefinitionContainer
    {
        private Dictionary<string, CommandDefinition> _commandDefinitions = new Dictionary<string, CommandDefinition>(StringComparer.Ordinal);

        /// <summary>
        /// Equality comparer used by the parser to resolve commands and their arguments.
        /// </summary>
        internal IEqualityComparer<string> StringEqualityComparer
        {
            get
            {
                return _commandDefinitions.Comparer;
            }
        }

        internal CommandDefinition Parent;

        /// <summary>
        /// Equality comparer used by the parser to resolve commands and their arguments.
        /// </summary>
        public bool CaseInsensitive
        {
            get
            {
                return _commandDefinitions.Comparer == StringComparer.OrdinalIgnoreCase;
            }
            set
            {
                if (_commandDefinitions.Count != 0) {
                    throw new InvalidOperationException("Cannot change default string equality comparer once commands have been defined.");
                }

                _commandDefinitions = new Dictionary<string, CommandDefinition>(value ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            }
        }

        /// <summary>
        /// Returns the collection of command definitions supported by this parser.
        /// </summary>
#if NET_40
        public IEnumerable<CommandDefinition> CommandDefinitions
#else
        public IReadOnlyList<CommandDefinition> CommandDefinitions
#endif
        {
            get
            {
                return _commandDefinitions.Values.ToArray();
            }
        }

        /// <summary>
        /// If true, basic app details such as name and version will be printed when the help command is invoked.
        /// </summary>
        public bool ShowAppDetailsInHelp { get; set; }

        /// <summary>
        /// Defines a command with the given name.
        /// </summary>
        public void DefineCommand(string name, Action<IndividualCommandDefinition> configureAction)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Command name cannot be empty.");

            if (_commandDefinitions.ContainsKey(name)) {
                throw new InvalidOperationException($"Command or command group '{name}' already defined.");
            }

            IndividualCommandDefinition definition = new IndividualCommandDefinition(name, Parent, StringEqualityComparer);

            configureAction(definition);

            _commandDefinitions.Add(name, definition);
        }

        /// <summary>
        /// Defines a group of commands with the given name.
        /// </summary>
        public void DefineCommandCollection(string name, Action<CommandCollectionDefinition> configureAction)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Command group name cannot be empty.");

            if (_commandDefinitions.ContainsKey(name)) {
                throw new InvalidOperationException($"Command or command group '{name}' already defined.");
            }

            CommandCollectionDefinition definition = new CommandCollectionDefinition(name, Parent, CaseInsensitive);

            configureAction(definition);

            _commandDefinitions.Add(name, definition);
        }

        /// <summary>
        /// Parses the command line args and returns the configured, ready-to-execute command.
        /// </summary>
        public ICommand Parse(params string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            string commandName = args.Length == 0 ? "" : args[0];

            if (args.Length == 1 && CommandSyntax.IsHelpSwitch(commandName, StringEqualityComparer)) {
                return new RootHelpCommand(this);
            }

            if (_commandDefinitions.TryGetValue(commandName, out CommandDefinition definition))
            {
                // Always skip first element.
                string[] argsMinusFirstElement = new string[args.Length - 1];

                Array.Copy(args, 1, argsMinusFirstElement, 0, args.Length - 1);

                return definition.Parse(argsMinusFirstElement);
            }

            throw new InvalidOperationException($"Unknown command '{commandName}'.");
        }
    }
}