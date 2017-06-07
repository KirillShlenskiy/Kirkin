using System;
using System.Collections.Generic;
using System.Linq;

using Kirkin.CommandLine.Commands;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Non-case sensitive command line argument parser.
    /// </summary>
    public sealed class CommandLineParser
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
        /// Defines a command with the given name.
        /// </summary>
        public void DefineCommand(string name, Action<CommandDefinition> configureAction)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Command name cannot be empty.");

            if (_commandDefinitions.ContainsKey(name)) {
                throw new InvalidOperationException($"Command '{name}' already defined.");
            }

            CommandDefinition definition = new CommandDefinition(name, StringEqualityComparer);

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
                return new GeneralHelpCommand(this);
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