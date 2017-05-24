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
        private readonly Dictionary<string, CommandDefinition> _commands = new Dictionary<string, CommandDefinition>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Returns the collection of command definitions supported by this parser.
        /// </summary>
        public IReadOnlyList<CommandDefinition> CommandDefinitions
        {
            get
            {
                return _commands.Values.ToArray();
            }
        }

        /// <summary>
        /// Defines a command with the given name.
        /// </summary>
        public void DefineCommand(string name, Action<CommandDefinition> configureAction)
        {
            if (_commands.ContainsKey(name)) {
                throw new InvalidOperationException($"Command '{name}' already defined.");
            }

            CommandDefinition definition = new CommandDefinition(name);

            configureAction(definition);

            _commands.Add(name, definition);
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

            if (_commands.TryGetValue(commandName, out CommandDefinition definition)) {
                return BuildCommand(definition, args.Skip(1).ToArray()); // TODO: Optimize.
            }

            throw new InvalidOperationException($"Unknown command: '{commandName}'.");
        }

        private static ICommand BuildCommand(CommandDefinition definition, string[] args) // ArraySlice<string>?
        {
            // TODO: Special handling for "--help", "/?".
            List<List<string>> chunks = new List<List<string>>();
            List<string> currentChunk = null;

            foreach (string arg in args)
            {
                if (currentChunk == null || arg.StartsWith("-") || arg.StartsWith("/"))
                {
                    currentChunk = new List<string>();

                    chunks.Add(currentChunk);
                }

                currentChunk.Add(arg);
            }

            HashSet<ICommandArg> seenParameters = new HashSet<ICommandArg>();
            Dictionary<string, object> argValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (List<string> chunk in chunks)
            {
                if (chunk[0].StartsWith("-") || chunk[0].StartsWith("/"))
                {
                    // Option.
                    ICommandArg option = null;

                    if (chunk[0].StartsWith("--"))
                    {
                        string fullName = chunk[0].Substring(2);

                        if (!definition.OptionsByFullName.TryGetValue(fullName, out option)) {
                            throw new InvalidOperationException($"Unable to find option with name '{fullName}'.");
                        }
                    }
                    else if (chunk[0].StartsWith("/"))
                    {
                        string fullName = chunk[0].Substring(1);

                        if (!definition.OptionsByFullName.TryGetValue(fullName, out option)) {
                            throw new InvalidOperationException($"Unable to find option with name '{fullName}'.");
                        }
                    }
                    else if (chunk[0].StartsWith("-"))
                    {
                        string shortName = chunk[0].Substring(1);

                        if (!definition.OptionsByShortName.TryGetValue(shortName, out option)) {
                            throw new InvalidOperationException($"Unable to find option with short name '{shortName}'.");
                        }
                    }

                    if (option == null) {
                        throw new InvalidOperationException($"Unhandled syntax token: '{chunk[0]}'.");
                    }

                    if (!seenParameters.Add(option)) {
                        throw new InvalidOperationException($"Duplicate option: '{chunk[0]}'.");
                    }

                    // TODO: Optimise.
                    argValues.Add(option.Name, option.GetValue(chunk.Skip(1).ToArray()));
                }
                else
                {
                    // Parameter.
                    if (definition.Parameter == null) {
                        throw new InvalidOperationException($"Command '{definition.Name}' does not define a parameter.");
                    }

                    if (!seenParameters.Add(definition.Parameter)) {
                        throw new InvalidOperationException("Duplicate parameter value detected.");
                    }

                    argValues.Add(definition.Parameter.Name, definition.Parameter.GetValue(chunk.ToArray()));
                }
            }

            if (definition.Parameter != null && !seenParameters.Contains(definition.Parameter)) {
                argValues.Add(definition.Parameter.Name, null);
            }

            foreach (ICommandArg option in definition.Options)
            {
                if (!seenParameters.Contains(option)) {
                    argValues.Add(option.Name, option.GetValue(null));
                }
            }

            return new DefaultCommand(definition, argValues);
        }
    }
}