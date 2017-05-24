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

            HashSet<Action<string[]>> seenProcessors = new HashSet<Action<string[]>>();

            foreach (List<string> chunk in chunks)
            {
                Action<string[]> processor = null;

                if (chunk[0].StartsWith("-") || chunk[0].StartsWith("/"))
                {
                    // Option.
                    if (chunk[0].StartsWith("--"))
                    {
                        string fullName = chunk[0].Substring(2);

                        if (!definition.ProcessorsByFullName.TryGetValue(fullName, out processor)) {
                            throw new InvalidOperationException($"Unable to find option with name '{fullName}'.");
                        }
                    }
                    else if (chunk[0].StartsWith("/"))
                    {
                        string fullName = chunk[0].Substring(1);

                        if (!definition.ProcessorsByFullName.TryGetValue(fullName, out processor)) {
                            throw new InvalidOperationException($"Unable to find option with name '{fullName}'.");
                        }
                    }
                    else if (chunk[0].StartsWith("-"))
                    {
                        string shortName = chunk[0].Substring(1);

                        if (!definition.ProcessorsByShortName.TryGetValue(shortName, out processor)) {
                            throw new InvalidOperationException($"Unable to find option with short name '{shortName}'.");
                        }
                    }

                    if (processor == null) {
                        throw new InvalidOperationException($"Unhandled syntax token: '{chunk[0]}'.");
                    }

                    if (!seenProcessors.Add(processor)) {
                        throw new InvalidOperationException($"Duplicate option: '{chunk[0]}'.");
                    }

                    processor(chunk.Skip(1).ToArray());
                }
                else
                {
                    // Parameter.
                    if (definition.Parameter == null) {
                        throw new InvalidOperationException($"Command '{definition.Name}' does not define a parameter.");
                    }

                    processor = definition.Parameter;

                    if (!seenProcessors.Add(processor)) {
                        throw new InvalidOperationException("Duplicate parameter value detected.");
                    }

                    processor(chunk.ToArray());
                }
            }

            HashSet<Action<string[]>> unusedProcessors = new HashSet<Action<string[]>>(definition.ProcessorsByFullName.Values);

            unusedProcessors.UnionWith(definition.ProcessorsByShortName.Values);
            unusedProcessors.ExceptWith(seenProcessors);

            foreach (Action<string[]> processor in unusedProcessors) {
                processor(null);
            }

            return new DefaultCommand(definition);
        }
    }
}