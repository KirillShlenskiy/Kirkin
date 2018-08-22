using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Kirkin.CommandLine.Parameters;
using Kirkin.CommandLine.Help;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Command definition.
    /// </summary>
    public sealed class CommandDefinition : ICommandDefinitionContainer
    {
        // Every command has zero or one parameter ("sync ==>extra<== --validate --log zzz.txt"),
        // and zero or more options/switches ("sync extra ==>--validate --log zzz.txt<==").
        internal CommandParameter MainParameter { get; private set; }
        internal readonly List<CommandParameter> Options = new List<CommandParameter>();

        private readonly Dictionary<string, CommandParameter> OptionsByFullName;
        private readonly Dictionary<string, CommandParameter> OptionsByShortName;
        private readonly CommandLineParser SubCommandParser;

        /// <summary>
        /// The name of the command being configured.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Human-readable command description.
        /// </summary>
        public string Help { get; set; }

        /// <summary>
        /// Gets all parameters and options defined by this command.
        /// </summary>
        public IEnumerable<CommandParameter> Parameters
        {
            get
            {
                if (MainParameter != null) yield return MainParameter;

                foreach (CommandParameter option in Options) {
                    yield return option;
                }
            }
        }


#if NET_40
        /// <summary>
        /// Returns the collection of subcommand definitions accessible via this instance.
        /// </summary>
        public IEnumerable<CommandDefinition> SubCommands => SubCommandParser.Commands;

        IEnumerable<CommandDefinition> ICommandDefinitionContainer.Commands => SubCommands;
#else
        /// <summary>
        /// Returns the collection of subcommand definitions accessible via this instance.
        /// </summary>
        public IReadOnlyList<CommandDefinition> SubCommands => SubCommandParser.Commands;

        IReadOnlyList<CommandDefinition> ICommandDefinitionContainer.Commands => SubCommands;
#endif
        internal IEqualityComparer<string> StringEqualityComparer => SubCommandParser.StringEqualityComparer;

        /// <summary>
        /// Parent command specified when this instance was created.
        /// </summary>
        internal CommandDefinition Parent { get; }

        /// <summary>
        /// Raised when <see cref="ICommand.Execute"/> is called on the command.
        /// </summary>
        public event EventHandler<CommandExecutedEventArgs> Executed;

        /// <summary>
        /// Creates a new standalone <see cref="IndividualCommandDefinition"/> instance.
        /// </summary>
        public CommandDefinition(bool caseInsensitive = false)
            : this("", null, caseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
        {
        }

        internal CommandDefinition(string name, CommandDefinition parent, IEqualityComparer<string> stringEqualityComparer)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (name.StartsWith("-")) throw new ArgumentException("Command name cannot start with a '-'.");
            if (name.StartsWith("/")) throw new ArgumentException("Command name cannot start with a '/'.");

            Name = name;
            Parent = parent;

            // Defaults.
            OptionsByFullName = new Dictionary<string, CommandParameter>(stringEqualityComparer);
            OptionsByShortName = new Dictionary<string, CommandParameter>(stringEqualityComparer);

            SubCommandParser = new CommandLineParser(stringEqualityComparer) {
                Parent = this
            };
        }

        /// <summary>
        /// Defines the main string parameter (unqualified value immediately following command name).
        /// </summary>
        public CommandParameter AddParameter(string name, string help = null)
        {
            if (SubCommands.Any()) {
                throw new InvalidOperationException("Cannot define parameter on a command which has sub-commands.");
            }

            MainCommandParameter parameter = new MainCommandParameter(name, help);

            MainParameter = parameter;

            return parameter;
        }

        /// <summary>
        /// Defines the main string parameter list (unqualified values immediately following command name).
        /// </summary>
        public CommandParameter AddParameterList(string name, string help = null)
        {
            if (SubCommands.Any()) {
                throw new InvalidOperationException("Cannot define parameter list on a command which has sub-commands.");
            }

            MainCommandParameterList parameterList = new MainCommandParameterList(name, help);

            MainParameter = parameterList;

            return parameterList;
        }

        /// <summary>
        /// Defines a string option, i.e. "--subscription main" or "-s main" or "/subscription main".
        /// </summary>
        public CommandParameter AddOption(string name, string shortName = null, bool positional = false, string help = null)
        {
            if (positional && SubCommands.Any()) {
                throw new InvalidOperationException("Cannot define positional parameter on a command which has sub-commands.");
            }

            OptionCommandParameter option = new OptionCommandParameter(name, shortName, positional, help);

            RegisterOption(option);

            return option;
        }

        /// <summary>
        /// Defines a string option, i.e. "--colours red green" or "-s red green".
        /// </summary>
        public CommandParameter AddOptionList(string name, string shortName = null, bool positional = false, string help = null)
        {
            if (positional && SubCommands.Any()) {
                throw new InvalidOperationException("Cannot define positional parameter list on a command which has sub-commands.");
            }

            OptionListCommandParameter optionList = new OptionListCommandParameter(name, shortName, positional, help);

            RegisterOption(optionList);

            return optionList;
        }

        /// <summary>
        /// Defines a boolean switch, i.e. "--validate" or "/validate true".
        /// </summary>
        public CommandParameter AddSwitch(string name, string shortName = null, string help = null)
        {
            SwitchCommandParameter option = new SwitchCommandParameter(name, shortName, help);

            RegisterOption(option);

            return option;
        }

        /// <summary>
        /// Defines a command with the given name.
        /// </summary>
        public void DefineCommand(string name, Action<CommandDefinition> configureAction)
        {
            if (MainParameter != null || Options.Any(o => o.IsPositionalParameter)) {
                throw new InvalidOperationException("Cannot define sub-commands on a command which has positional args or main parameter.");
            }

            SubCommandParser.DefineCommand(name, configureAction);
        }

        /// <summary>
        /// Parses the given args collection and produces a ready-to-use <see cref="ICommand"/> instance.
        /// </summary>
        internal ICommand Parse(string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            if (args.Length == 0 && SubCommands.Any() && Executed == null)
            {
                // Rewrite as a --help command.
                args = new[] { "--help" };

                return SubCommandParser.Parse(args);
            }

            IEqualityComparer<string> stringEqualityComparer = OptionsByFullName.Comparer;

            if (args.Length == 1 && CommandSyntax.IsHelpSwitch(args[0], stringEqualityComparer)) {
                return new CommandDefinitionHelpCommand(this);
            }

            List<List<string>> tokenGroups = new List<List<string>>();
            List<string> currentTokenGroup = null;

            foreach (string arg in args)
            {
                if (currentTokenGroup == null || arg.StartsWith("-") || arg.StartsWith("/"))
                {
                    currentTokenGroup = new List<string>();

                    tokenGroups.Add(currentTokenGroup);
                }

                //int nameValueSplitIndex = arg.IndexOf(':');

                //if (nameValueSplitIndex == -1) nameValueSplitIndex = arg.IndexOf('=');

                //if (nameValueSplitIndex != -1)
                //{
                //    // Name/value pair.
                //    currentTokenGroup.Add(arg.Substring(0, nameValueSplitIndex));
                //    currentTokenGroup.Add(arg.Substring(nameValueSplitIndex + 1));
                //}
                //else
                {
                    currentTokenGroup.Add(arg);
                }
            }

            HashSet<CommandParameter> seenParameters = new HashSet<CommandParameter>();
            Dictionary<string, object> argValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (List<string> chunk in tokenGroups)
            {
                if (chunk[0].StartsWith("-") || chunk[0].StartsWith("/"))
                {
                    // Option.
                    CommandParameter option = null;

                    if (chunk[0].StartsWith("--"))
                    {
                        string fullName = chunk[0].Substring(2);

                        if (!OptionsByFullName.TryGetValue(fullName, out option)) {
                            throw new InvalidOperationException($"Unknown option '{fullName}'.");
                        }
                    }
                    else if (chunk[0].StartsWith("/"))
                    {
                        string fullName = chunk[0].Substring(1);

                        if (!OptionsByFullName.TryGetValue(fullName, out option)) {
                            throw new InvalidOperationException($"Unknown option '{fullName}'.");
                        }
                    }
                    else if (chunk[0].StartsWith("-"))
                    {
                        string shortName = chunk[0].Substring(1);

                        if (!OptionsByShortName.TryGetValue(shortName, out option)) {
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
                    // Parameter, positional arg or subcommand.
                    if (MainParameter == null || !MainParameter.SupportsMultipleValues && chunk.Count > 1)
                    {
                        // TODO: Parser == null check?
                        if (SubCommands.Any()) {
                            return SubCommandParser.Parse(args);
                        }

                        // Positional args.
                        List<CommandParameter> positionalParams = Parameters
                            .Where(d => d.IsPositionalParameter)
                            .ToList();

                        int lastPositionalArgIndex = -1;

                        while (chunk.Count != 0)
                        {
                            if (++lastPositionalArgIndex > positionalParams.Count - 1) {
                                throw new InvalidOperationException("Too many positional args.");
                            }

                            CommandParameter option = positionalParams[lastPositionalArgIndex];

                            if (option.SupportsMultipleValues) {
                                throw new ArgumentException("Multi-valued positional args not supported.");
                            }

                            // Parse single arg.
                            List<string> singleArg = new List<string> { chunk[0] };

                            seenParameters.Add(option);
                            chunk.RemoveAt(0);
                            argValues.Add(option.Name, option.ParseArgs(singleArg));
                        }
                    }
                    else
                    {
                        if (!seenParameters.Add(MainParameter)) {
                            throw new InvalidOperationException("Duplicate parameter value.");
                        }

                        argValues.Add(MainParameter.Name, MainParameter.ParseArgs(chunk));
                    }
                }
            }

            foreach (CommandParameter param in Parameters)
            {
                if (!seenParameters.Contains(param)) {
                    argValues.Add(param.Name, param.GetDefaultValue());
                }
            }

            return new DefaultCommand(this, new CommandArguments(this, argValues));
        }

        private void RegisterOption(CommandParameter option)
        {
            if (OptionsByFullName.ContainsKey(option.Name)) {
                throw new InvalidOperationException($"Duplicate option name: '{option.Name}'.");
            }

            if (!string.IsNullOrEmpty(option.ShortName) && OptionsByShortName.ContainsKey(option.ShortName)) {
                throw new InvalidOperationException($"Duplicate option short name: '{option.Name}'.");
            }

            OptionsByFullName.Add(option.Name, option);
            
            if (!string.IsNullOrEmpty(option.ShortName)) {
                OptionsByShortName.Add(option.ShortName, option);
            }

            Options.Add(option);
        }

        internal void RaiseExecutedEvent(ICommand command, CommandArguments args)
        {
            if (Executed != null)
            {
                CommandExecutedEventArgs e = new CommandExecutedEventArgs(command, args);

                Executed(command, e);
            }
        }

        internal IHelpCommand CreateHelpCommand()
        {
            return new CommandDefinitionHelpCommand(this);
        }

        /// <summary>
        /// Returns the formal command definition.
        /// </summary>
        public override string ToString()
        {
            // TODO: Parser != null check?
            if (SubCommands.Any()) {
                return $"{Name} <command>";
            }

            StringBuilder sb = new StringBuilder();

            sb.Append(Name);

            if (MainParameter != null)
            {
                sb.Append(' ');
                sb.Append(MainParameter);
            }

            foreach (CommandParameter option in Options)
            {
                sb.Append(" [");
                sb.Append((option as IParameterFormattable)?.ToShortString() ?? option.ToString());
                sb.Append(']');
            }

            return sb.ToString();
        }
    }
}