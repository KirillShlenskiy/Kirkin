using System;
using System.Collections.Generic;
using System.Text;

using Kirkin.CommandLine.Parameters;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Builder type used to configure commands.
    /// </summary>
    public sealed class CommandDefinition
    {
        // Every command has zero or one parameter ("sync ==>extra<== --validate --log zzz.txt"),
        // and zero or more options/switches ("sync extra ==>--validate --log zzz.txt<==").
        internal ICommandParameterDefinition Parameter { get; private set; }
        internal readonly List<ICommandParameterDefinition> Options = new List<ICommandParameterDefinition>();
        internal readonly Dictionary<string, ICommandParameterDefinition> OptionsByFullName;
        internal readonly Dictionary<string, ICommandParameterDefinition> OptionsByShortName;

        /// <summary>
        /// The name of the command being configured.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Human-readable command description.
        /// </summary>
        public string Help { get; set; }

        /// <summary>
        /// Raised when <see cref="ICommand.Execute"/> is called on the command.
        /// </summary>
        public event EventHandler<CommandExecutedEventArgs> Executed;

        internal CommandDefinition(string name, IEqualityComparer<string> stringEqualityComparer)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Command name cannot be empty.");

            Name = name;
            OptionsByFullName = new Dictionary<string, ICommandParameterDefinition>(stringEqualityComparer);
            OptionsByShortName = new Dictionary<string, ICommandParameterDefinition>(stringEqualityComparer);
        }

        internal void OnExecuted(ICommand command, IDictionary<string, object> args)
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
        public ICommandParameter DefineParameterList(string name, string help = null)
        {
            CommandParameterList parameterList = new CommandParameterList(name, help);

            Parameter = parameterList;

            return parameterList;
        }

        /// <summary>
        /// Defines a string option, i.e. "--subscription main" or "-s main" or "/subscription main".
        /// </summary>
        public ICommandParameter DefineOption(string name, string shortName = null, string help = null)
        {
            OptionCommandParameter option = new OptionCommandParameter(name, shortName, help);

            RegisterOption(option);

            return option;
        }

        /// <summary>
        /// Defines a string option, i.e. "--colours red green" or "-s red green".
        /// </summary>
        public ICommandParameter DefineOptionList(string name, string shortName = null, string help = null)
        {
            OptionListCommandParameter optionList = new OptionListCommandParameter(name, shortName, help);

            RegisterOption(optionList);

            return optionList;
        }

        /// <summary>
        /// Defines a boolean switch, i.e. "--validate" or "/validate true".
        /// </summary>
        public ICommandParameter DefineSwitch(string name, string shortName = null, string help = null)
        {
            SwitchCommandParameter option = new SwitchCommandParameter(name, shortName, help);

            RegisterOption(option);

            return option;
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

        public override string ToString()
        {
            // replmon sync <subscription> [-v] [-l <arg>] [-p <arg>...]
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
                sb.Append(option);
                sb.Append(']');
            }

            return sb.ToString();
        }
    }
}