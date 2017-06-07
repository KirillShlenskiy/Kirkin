using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public IEqualityComparer<string> StringEqualityComparer
        {
            get
            {
                return _commandDefinitions.Comparer;
            }
            set
            {
                if (_commandDefinitions.Count != 0) {
                    throw new InvalidOperationException("Cannot change default string equality comparer once commands have been defined.");
                }

                _commandDefinitions = new Dictionary<string, CommandDefinition>(value);
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
        public void DefineCommand(string name, string help, Action<CommandDefinition> configureAction)
        {
            if (_commandDefinitions.ContainsKey(name)) {
                throw new InvalidOperationException($"Command '{name}' already defined.");
            }

            CommandDefinition definition = new CommandDefinition(name, help, StringEqualityComparer);

            configureAction(definition);

            _commandDefinitions.Add(name, definition);
        }

        /// <summary>
        /// Parses the command line args and returns the configured, ready-to-execute command.
        /// </summary>
        public ICommand Parse(params string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (args.Length == 0) return new HelpCommand(this);

            string commandName = args[0];

            if (args.Length == 1 && (string.IsNullOrEmpty(commandName) || StringEqualityComparer.Equals(commandName, "--help") || StringEqualityComparer.Equals(commandName, "/?"))) {
                return new HelpCommand(this);
            }

            if (_commandDefinitions.TryGetValue(commandName, out CommandDefinition definition)) {
                return BuildCommand(definition, args);
            }

            throw new InvalidOperationException($"Unknown command '{commandName}'.");
        }

        private static ICommand BuildCommand(CommandDefinition definition, string[] args)
        {
            // TODO: Special handling for "--help", "/?".
            List<List<string>> tokenGroups = new List<List<string>>();
            List<string> currentTokenGroup = null;

            for (int i = 1; i < args.Length; i++) // Always skip first element.
            {
                string arg = args[i];

                if (currentTokenGroup == null || arg.StartsWith("-") || arg.StartsWith("/"))
                {
                    currentTokenGroup = new List<string>();

                    tokenGroups.Add(currentTokenGroup);
                }

                int nameValueSplitIndex = arg.IndexOf(':');

                if (nameValueSplitIndex == -1) nameValueSplitIndex = arg.IndexOf('=');

                if (nameValueSplitIndex != -1)
                {
                    // Name/value pair.
                    currentTokenGroup.Add(arg.Substring(0, nameValueSplitIndex));
                    currentTokenGroup.Add(arg.Substring(nameValueSplitIndex + 1));
                }
                else
                {
                    currentTokenGroup.Add(arg);
                }
            }

            HashSet<ICommandParameter> seenParameters = new HashSet<ICommandParameter>();
            Dictionary<string, object> argValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (List<string> chunk in tokenGroups)
            {
                if (chunk[0].StartsWith("-") || chunk[0].StartsWith("/"))
                {
                    // Option.
                    ICommandParameterDefinition option = null;

                    if (chunk[0].StartsWith("--"))
                    {
                        string fullName = chunk[0].Substring(2);

                        if (!definition.OptionsByFullName.TryGetValue(fullName, out option)) {
                            throw new InvalidOperationException($"Unknown option '{fullName}'.");
                        }
                    }
                    else if (chunk[0].StartsWith("/"))
                    {
                        string fullName = chunk[0].Substring(1);

                        if (!definition.OptionsByFullName.TryGetValue(fullName, out option)) {
                            throw new InvalidOperationException($"Unknown option '{fullName}'.");
                        }
                    }
                    else if (chunk[0].StartsWith("-"))
                    {
                        string shortName = chunk[0].Substring(1);

                        if (!definition.OptionsByShortName.TryGetValue(shortName, out option)) {
                            throw new InvalidOperationException($"Unknown option '{shortName}'.");
                        }
                    }

                    if (option == null) {
                        throw new InvalidOperationException($"Unhandled syntax token '{chunk[0]}'.");
                    }

                    if (!seenParameters.Add(option)) {
                        throw new InvalidOperationException($"Duplicate option '{chunk[0]}'.");
                    }

                    chunk.RemoveAt(0);
                    argValues.Add(option.Name, option.ParseArgs(chunk));
                }
                else
                {
                    // Parameter.
                    if (definition.Parameter == null) {
                        throw new InvalidOperationException($"Command '{definition.Name}' does not define a parameter.");
                    }

                    if (!seenParameters.Add(definition.Parameter)) {
                        throw new InvalidOperationException("Duplicate parameter value.");
                    }

                    argValues.Add(definition.Parameter.Name, definition.Parameter.ParseArgs(chunk));
                }
            }

            if (definition.Parameter != null && !seenParameters.Contains(definition.Parameter)) {
                argValues.Add(definition.Parameter.Name, definition.Parameter.GetDefaultValue());
            }

            foreach (ICommandParameterDefinition option in definition.Options)
            {
                if (!seenParameters.Contains(option)) {
                    argValues.Add(option.Name, option.GetDefaultValue());
                }
            }

            return new DefaultCommand(definition, argValues);
        }

        /// <summary>
        /// Builds the help string.
        /// </summary>
        private string RenderHelpText()
        {
            const int screenWidth = 72;
            int maxCommandWidth = 0;

            foreach (CommandDefinition commandDefinition in _commandDefinitions.Values)
            {
                if (commandDefinition.Name.Length > maxCommandWidth) {
                    maxCommandWidth = commandDefinition.Name.Length;
                }
            }

            StringBuilder sb = new StringBuilder();
            const string tab = "    ";
            int leftColumnWidth = tab.Length * 2 + maxCommandWidth;

            foreach (CommandDefinition commandDefinition in _commandDefinitions.Values)
            {
                sb.Append(tab);
                sb.Append(commandDefinition.Name.PadRight(maxCommandWidth));
                sb.Append(tab);

                for (int i = 0; i < commandDefinition.Help.Length; i++)
                {
                    if (i > 0 && i % (screenWidth - leftColumnWidth) == 0)
                    {
                        sb.AppendLine();
                        sb.Append(' ', leftColumnWidth);
                    }

                    sb.Append(commandDefinition.Help[i]);
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return RenderHelpText();
        }
    }
}