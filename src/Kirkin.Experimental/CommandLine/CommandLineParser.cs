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
        private readonly Dictionary<string, CommandSyntax> _commands = new Dictionary<string, CommandSyntax>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Defines a command with the given name.
        /// </summary>
        public void DefineCommand(string name, Action<CommandSyntax> configureAction)
        {
            if (_commands.ContainsKey(name)) {
                throw new InvalidOperationException($"Command '{name}' already defined.");
            }

            CommandSyntax syntax = new CommandSyntax(name);

            configureAction(syntax);

            _commands.Add(name, syntax);
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

            if (_commands.TryGetValue(commandName, out CommandSyntax syntax)) {
                return BuildCommand(syntax, args.Skip(1).ToArray()); // TODO: Optimize.
            }

            throw new InvalidOperationException($"Unknown command: '{commandName}'.");
        }

        private static ICommand BuildCommand(CommandSyntax syntax, string[] args) // ArraySlice<string>?
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

                        if (!syntax.ProcessorsByFullName.TryGetValue(fullName, out processor)) {
                            throw new InvalidOperationException($"Unable to find option with name '{fullName}'.");
                        }
                    }
                    else if (chunk[0].StartsWith("/"))
                    {
                        string fullName = chunk[0].Substring(1);

                        if (!syntax.ProcessorsByFullName.TryGetValue(fullName, out processor)) {
                            throw new InvalidOperationException($"Unable to find option with name '{fullName}'.");
                        }
                    }
                    else if (chunk[0].StartsWith("-"))
                    {
                        string shortName = chunk[0].Substring(1);

                        if (!syntax.ProcessorsByShortName.TryGetValue(shortName, out processor)) {
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
                    if (syntax.Parameter == null) {
                        throw new InvalidOperationException($"Command '{syntax.Name}' does not define a parameter.");
                    }

                    processor = syntax.Parameter;

                    if (!seenProcessors.Add(processor)) {
                        throw new InvalidOperationException("Duplicate parameter value detected.");
                    }

                    processor(chunk.ToArray());
                }
            }

            HashSet<Action<string[]>> unusedProcessors = new HashSet<Action<string[]>>(syntax.ProcessorsByFullName.Values);

            unusedProcessors.UnionWith(syntax.ProcessorsByShortName.Values);
            unusedProcessors.ExceptWith(seenProcessors);

            foreach (Action<string[]> processor in unusedProcessors) {
                processor(null);
            }

            return new SyntaxCommand(syntax);
        }
    }
}