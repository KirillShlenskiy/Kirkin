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
        internal CommandParameter Parameter { get; private set; }
        internal readonly List<ICommandParameterDefinition> Options = new List<ICommandParameterDefinition>();
        internal readonly Dictionary<string, ICommandParameterDefinition> OptionsByFullName = new Dictionary<string, ICommandParameterDefinition>(StringComparer.OrdinalIgnoreCase);
        internal readonly Dictionary<string, ICommandParameterDefinition> OptionsByShortName = new Dictionary<string, ICommandParameterDefinition>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The name of the command being configured.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Raised when <see cref="ICommand.Execute"/> is called on the command.
        /// </summary>
        public event EventHandler<CommandExecutedEventArgs> Executed;

        internal CommandDefinition(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Command name cannot be empty.");

            Name = name;
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
        public ICommandParameter DefineParameter(string name)
        {
            CommandParameter parameter = new CommandParameter(name);

            Parameter = parameter;

            return parameter;
        }

        /// <summary>
        /// Defines a string option, i.e. "--subscription main" or "-s main" or "/subscription main".
        /// </summary>
        public ICommandParameter DefineOption(string name, string shortName = null)
        {
            OptionCommandParameter option = new OptionCommandParameter(name, shortName);

            RegisterOption(option);

            return option;
        }

        /// <summary>
        /// Defines a boolean switch, i.e. "--validate" or "/validate true".
        /// </summary>
        public ICommandParameter DefineSwitch(string name, string shortName = null)
        {
            SwitchCommandParameter option = new SwitchCommandParameter(name, shortName);

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
            // replmon sync [-v] [-l <arg>] [-p <arg>...] [--] <subscription>
            StringBuilder sb = new StringBuilder();

            sb.Append(Name);

            if (Parameter != null) {
                sb.Append($" <{Parameter.Name}>");
            }

            foreach (ICommandParameter option in Options)
            {
                sb.Append(" [");

                if (option.ShortName!= null) {
                    sb.Append($"-{option.ShortName}|");
                }

                if (option is SwitchCommandParameter)
                {
                    // Switches don't have arguments.
                    sb.Append($"--{option.Name}]");
                }
                else
                {
                    sb.Append($"--{option.Name} <arg>]");
                }
            }

            return sb.ToString();
        }
    }
}