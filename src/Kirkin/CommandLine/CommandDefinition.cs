using System;
using System.Collections.Generic;
using System.Text;

using Kirkin.CommandLine.Commands;
using Kirkin.CommandLine.Parameters;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Builder type used to configure commands.
    /// </summary>
    public sealed class CommandDefinition : CommandDefinitionBase
    {
        // Every command has zero or one parameter ("sync ==>extra<== --validate --log zzz.txt"),
        // and zero or more options/switches ("sync extra ==>--validate --log zzz.txt<==").
        internal ICommandParameterDefinition Parameter { get; private set; }
        internal readonly List<ICommandParameterDefinition> Options = new List<ICommandParameterDefinition>();
        private readonly Dictionary<string, ICommandParameterDefinition> OptionsByFullName;
        private readonly Dictionary<string, ICommandParameterDefinition> OptionsByShortName;

        /// <summary>
        /// Raised when <see cref="ICommand.Execute"/> is called on the command.
        /// </summary>
        public event EventHandler<CommandExecutedEventArgs> Executed;

        /// <summary>
        /// Creates a new standalone <see cref="CommandDefinition"/> instance.
        /// </summary>
        public CommandDefinition(bool caseInsensitive = false)
            : this("", caseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
        {
        }

        internal CommandDefinition(string name, IEqualityComparer<string> stringEqualityComparer)
            : base(name)
        {
            if (name.StartsWith("-")) throw new ArgumentException("Command name cannot start with a '-'.");
            if (name.StartsWith("/")) throw new ArgumentException("Command name cannot start with a '/'.");

            OptionsByFullName = new Dictionary<string, ICommandParameterDefinition>(stringEqualityComparer);
            OptionsByShortName = new Dictionary<string, ICommandParameterDefinition>(stringEqualityComparer);
        }

        internal void OnExecuted(ICommand command, CommandArguments args)
        {
            if (Executed != null)
            {
                CommandExecutedEventArgs e = new CommandExecutedEventArgs(command, args);

                Executed(command, e);
            }
        }

        /// <summary>
        /// Defines a string parameter (unqualified value immediately following command name).
        /// </summary>
        public ICommandParameter DefineParameter(string name, string help = null)
        {
            CommandParameter parameter = new CommandParameter(name, help);

            Parameter = parameter;

            return parameter;
        }

        /// <summary>
        /// Defines a string parameter list (unqualified values immediately following command name).
        /// </summary>
        public ICommandParameter DefineParameterList(string name, string help = null, bool positional = false)
        {
            CommandParameterList parameterList = new CommandParameterList(name, help, positional);

            Parameter = parameterList;

            return parameterList;
        }

        /// <summary>
        /// Defines a string option, i.e. "--subscription main" or "-s main" or "/subscription main".
        /// </summary>
        public ICommandParameter DefineOption(string name, string shortName = null, string help = null, bool positional = false)
        {
            OptionCommandParameter option = new OptionCommandParameter(name, shortName, help, positional);

            RegisterOption(option);

            return option;
        }

        /// <summary>
        /// Defines a string option, i.e. "--colours red green" or "-s red green".
        /// </summary>
        public ICommandParameter DefineOptionList(string name, string shortName = null, string help = null, bool positional = false)
        {
            OptionListCommandParameter optionList = new OptionListCommandParameter(name, shortName, help, positional);

            RegisterOption(optionList);

            return optionList;
        }

        /// <summary>
        /// Defines a boolean switch, i.e. "--validate" or "/validate true".
        /// </summary>
        public ICommandParameter DefineSwitch(string name, string shortName = null, string help = null, bool positional = false)
        {
            SwitchCommandParameter option = new SwitchCommandParameter(name, shortName, help, positional);

            RegisterOption(option);

            return option;
        }

        /// <summary>
        /// Parses the given args collection and produces a ready-to-use <see cref="ICommand"/> instance.
        /// </summary>
        public override ICommand Parse(string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

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
                    // Parameter or positional args.
                    if (Parameter == null || !Parameter.SupportsMultipleValues && chunk.Count > 1)
                    {
                        // Positional args.
                        List<ICommandParameterDefinition> positionalParams = new List<ICommandParameterDefinition>();

                        if (Parameter != null) {
                            positionalParams.Add(Parameter);
                        }

                        foreach (ICommandParameterDefinition option in Options)
                        {
                            if (option.IsPositionalParameter) {
                                positionalParams.Add(option);
                            }
                        }

                        int lastPositionalArgIndex = -1;

                        while (chunk.Count != 0)
                        {
                            if (++lastPositionalArgIndex > positionalParams.Count - 1) {
                                throw new InvalidOperationException("Too many positional args.");
                            }

                            ICommandParameterDefinition option = positionalParams[lastPositionalArgIndex];

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
                        if (!seenParameters.Add(Parameter)) {
                            throw new InvalidOperationException("Duplicate parameter value.");
                        }

                        argValues.Add(Parameter.Name, Parameter.ParseArgs(chunk));
                    }
                }
            }

            if (Parameter != null && !seenParameters.Contains(Parameter)) {
                argValues.Add(Parameter.Name, Parameter.GetDefaultValue());
            }

            foreach (ICommandParameterDefinition option in Options)
            {
                if (!seenParameters.Contains(option)) {
                    argValues.Add(option.Name, option.GetDefaultValue());
                }
            }

            return new DefaultCommand(this, new CommandArguments(this, argValues));
        }

        private void RegisterOption(ICommandParameterDefinition option)
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

        private protected override IHelpCommand CreateHelpCommand()
        {
            return new CommandDefinitionHelpCommand(this);
        }

        /// <summary>
        /// Returns the formal command definition.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Name);

            if (Parameter != null)
            {
                sb.Append(' ');
                sb.Append(Parameter);
            }

            foreach (ICommandParameter option in Options)
            {
                sb.Append(" [");
                sb.Append((option as IParameterFormattable)?.ToShortString() ?? option.ToString());
                sb.Append(']');
            }

            return sb.ToString();
        }
    }
}