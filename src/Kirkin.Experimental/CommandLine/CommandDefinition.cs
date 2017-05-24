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
        internal readonly List<ICommandParameter> Options = new List<ICommandParameter>();
        internal readonly Dictionary<string, ICommandParameter> OptionsByFullName = new Dictionary<string, ICommandParameter>(StringComparer.OrdinalIgnoreCase);
        internal readonly Dictionary<string, ICommandParameter> OptionsByShortName = new Dictionary<string, ICommandParameter>(StringComparer.OrdinalIgnoreCase);

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
        /// Defines a string option, i.e. "--subscription main" or "-s main" or "/subscription main".
        /// </summary>
        public void DefineOption(string name, string shortName = null)
        {
            ICommandParameter option = new OptionCommandParameter(name, shortName);

            RegisterOption(option);
        }

        /// <summary>
        /// Defines a string parameter (unqualified value immediately following command name).
        /// </summary>
        public void DefineParameter(string name)
        {
            CommandParameter parameter = new CommandParameter(name);

            Parameter = parameter;
        }

        /// <summary>
        /// Defines a boolean switch, i.e. "--validate" or "/validate true".
        /// </summary>
        public void DefineSwitch(string name, string shortName = null)
        {
            ICommandParameter option = new SwitchCommandParameter(name, shortName);

            RegisterOption(option);
        }

        private void RegisterOption(ICommandParameter option)
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