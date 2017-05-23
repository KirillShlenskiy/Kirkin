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
        private readonly Dictionary<string, Func<string[], ICommand>> _commandFactories = new Dictionary<string, Func<string[], ICommand>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Defines a command with the given name.
        /// </summary>
        public void DefineCommand(string name, Action<CommandSyntax> configureAction)
        {
            if (_commandFactories.ContainsKey(name)) {
                throw new InvalidOperationException($"Command '{name}' already defined.");
            }

            CommandSyntax builder = new CommandSyntax(name);

            configureAction(builder);

            _commandFactories.Add(name, args =>
            {
                builder.BuildCommand(args);

                return new DelegateCommand(name, () => builder.OnExecuted());
            });
        }

        /// <summary>
        /// Parses the command line args and returns the configured, ready-to-execute command.
        /// </summary>
        public ICommand Parse(params string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (args.Length == 0) return new HelpCommand();

            // TODO: Check reserved keywords (i.e. "--help", "/?").
            string commandName = args[0];

            if (_commandFactories.TryGetValue(commandName, out Func<string[], ICommand> commandFactory)) {
                return commandFactory(args.Skip(1).ToArray()); // TODO: Optimize.
            }

            throw new InvalidOperationException($"Unknown command: '{commandName}'.");
        }
    }
}