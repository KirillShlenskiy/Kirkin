using System;
using System.Collections.Generic;

using Kirkin.CommandLine.Commands;

namespace Kirkin.CommandLine
{
    public sealed class CommandLineParser
    {
        private readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase);

        public void DefineCommand(string name, Func<CommandSyntax, Action> configureAction)
        {
            if (_commands.ContainsKey(name)) {
                throw new InvalidOperationException($"Command '{name}' already defined.");
            }

            CommandSyntax builder = new CommandSyntax(name);

            configureAction(builder);

            _commands.Add(name, builder.BuildCommand());
        }

        //public Command DefineCommand(string name)
        //{
        //    Command command = new Command(name);

        //    Commands.Add(command);

        //    return command;
        //}

        public ICommand Parse(params string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (args.Length == 0) return new HelpCommand();

            // TODO: Check reserved keywords (i.e. "--help", "/?").
            string commandName = args[0];

            if (_commands.TryGetValue(commandName, out ICommand command)) {
                return command;
            }

            throw new InvalidOperationException($"Unknown command: '{commandName}'.");
        }
    }
}