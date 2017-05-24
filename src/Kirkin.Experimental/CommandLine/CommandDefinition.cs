using System;
using System.Collections.Generic;
using System.Text;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Builder type used to configure commands.
    /// </summary>
    public sealed class CommandDefinition
    {
        internal ICommandArg Parameter { get; private set; }
        internal readonly List<ICommandArg> Options = new List<ICommandArg>();
        internal readonly Dictionary<string, ICommandArg> OptionsByFullName = new Dictionary<string, ICommandArg>(StringComparer.OrdinalIgnoreCase);
        internal readonly Dictionary<string, ICommandArg> OptionsByShortName = new Dictionary<string, ICommandArg>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The name of the command being configured.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Raised when <see cref="ICommand.Execute"/> is called on the command.
        /// When this event fires, it is safe to access command argument values.
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
            CommandArg<string> option = new CommandArg<string>(name, shortName, args =>
            {
                if (args == null || args.Length == 0) return null;

                if (args.Length > 1) {
                    throw new InvalidOperationException($"Multiple argument values are not supported for option '{name}'.");
                }

                return args[0];
            });

            RegisterArg(option);
        }

        /// <summary>
        /// Defines a string parameter.
        /// </summary>
        public void DefineParameter(string name)
        {
            CommandArg<string> parameter = new CommandArg<string>(name, null, args =>
            {
                if (args == null || args.Length == 0) return null;

                if (args.Length > 1) {
                    throw new InvalidOperationException($"Multiple argument values are not supported for option '{name}'.");
                }

                return args[0];
            });

            Parameter = parameter;
        }

        /// <summary>
        /// Defines a boolean switch, i.e. "--validate" or "/validate true".
        /// </summary>
        public void DefineSwitch(string name, string shortName = null)
        {
            CommandArg<bool> option = new CommandArg<bool>(name, shortName, args =>
            {
                if (args == null) return false;

                if (args.Length > 1) {
                    throw new InvalidOperationException($"Multiple argument values are not supported for switch '{name}'.");
                }

                return args.Length == 0 // A switch does not need to have a value to be true.
                    || Convert.ToBoolean(args[0]);
            });

            RegisterArg(option);
        }

        private void RegisterArg(ICommandArg arg)
        {
            if (OptionsByFullName.ContainsKey(arg.Name)) {
                throw new InvalidOperationException($"Duplicate option name: '{arg.Name}'.");
            }

            if (!string.IsNullOrEmpty(arg.ShortName) && OptionsByShortName.ContainsKey(arg.ShortName)) {
                throw new InvalidOperationException($"Duplicate option short name: '{arg.Name}'.");
            }

            OptionsByFullName.Add(arg.Name, arg);
            
            if (!string.IsNullOrEmpty(arg.ShortName)) {
                OptionsByShortName.Add(arg.ShortName, arg);
            }

            Options.Add(arg);
        }

        public override string ToString()
        {
            // replmon sync [-v] [-l <arg>] [-p <arg>...] [--] <subscription>
            StringBuilder sb = new StringBuilder();

            sb.Append(Name);

            if (Parameter != null) {
                sb.Append($" <{Parameter.Name}>");
            }

            foreach (ICommandArg option in Options)
            {
                sb.Append(" [");

                if (option.ShortName!= null) {
                    sb.Append($"-{option.ShortName}|");
                }

                sb.Append($"--{option.Name} <arg>]");
            }

            return sb.ToString();
        }
    }
}