using System;
using System.Collections.Generic;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Builder type used to configure commands.
    /// </summary>
    public sealed class CommandSyntax
    {
        internal readonly Dictionary<string, Action<string[]>> ProcessorsByFullName = new Dictionary<string, Action<string[]>>(StringComparer.OrdinalIgnoreCase);
        internal readonly Dictionary<string, Action<string[]>> ProcessorsByShortName = new Dictionary<string, Action<string[]>>(StringComparer.OrdinalIgnoreCase);
        internal readonly List<ICommandArg> Arguments = new List<ICommandArg>();

        /// <summary>
        /// The name of the command being configured.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Raised when <see cref="ICommand.Execute"/> is called on the command.
        /// When this event fires, it is safe to access command argument values.
        /// </summary>
        public event Action Executed;

        internal CommandSyntax(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Command name cannot be empty.");

            Name = name;
        }

        internal void OnExecuted()
        {
            Executed?.Invoke();
        }

        /// <summary>
        /// Defines a string option, i.e. "--subscription main" or "-s main" or "/subscription main".
        /// </summary>
        public CommandArg<string> DefineOption(string name, string shortName = null)
        {
            Func<string> container = DefineCustomOption(name, shortName, value => value);
            CommandArg<string> arg = new CommandArg<string>(name, shortName, () => container());

            Arguments.Add(arg);

            return arg;
        }

        /// <summary>
        /// Defines a boolean switch, i.e. "--validate" or "/validate true".
        /// </summary>
        public CommandArg<bool> DefineSwitch(string name, string shortName = null)
        {
            Func<string> container = DefineCustomOption(name, shortName, value => value);

            CommandArg<bool> arg = new CommandArg<bool>(name, shortName, () =>
            {
                string value = container();

                return value != null && (value.Length == 0 || value.Equals("true", StringComparison.OrdinalIgnoreCase));
            });

            Arguments.Add(arg);

            return arg;
        }

        internal Func<T> DefineCustomOption<T>(string name, string shortName, Func<string, T> valueConverter)
        {
            return DefineCustomOptionList(name, shortName, args =>
            {
                if (args.Length == 0) return valueConverter(string.Empty);
                if (args.Length == 1) return valueConverter(args[0]);

                throw new InvalidOperationException($"Multiple argument values are not supported for option '{name}'.");
            });
        }

        internal Func<T> DefineCustomOptionList<T>(string name, string shortName, Func<string[], T> valueConverter)
        {
            bool ready = false;
            T value = default(T);

            Action<string[]> processor = args =>
            {
                value = (args == null) ? default(T) : valueConverter(args);
                ready = true;
            };

            ProcessorsByFullName.Add(name, processor);

            if (!string.IsNullOrEmpty(shortName)) ProcessorsByShortName.Add(shortName, processor);

            return () =>
            {
                if (!ready) {
                    throw new InvalidOperationException("Value is undefined until Execute is called on the command.");
                }

                return value;
            };
        }

        public override string ToString()
        {
            return Name;
        }
    }
}