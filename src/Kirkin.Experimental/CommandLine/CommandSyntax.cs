using System;
using System.Collections.Generic;
using System.Linq;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Builder type used to configure commands.
    /// </summary>
    public sealed class CommandSyntax
    {
        private readonly Dictionary<string, Action<string[]>> _processorsByFullName = new Dictionary<string, Action<string[]>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Action<string[]>> _processorsByShortName = new Dictionary<string, Action<string[]>>(StringComparer.OrdinalIgnoreCase);

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
        public Arg<string> DefineOption(string name, string shortName = null)
        {
            Func<string> container = DefineCustomOption(name, shortName, value => value);

            return new Arg<string>(() => container());
        }

        /// <summary>
        /// Defines a boolean switch, i.e. "--validate" or "/validate true".
        /// </summary>
        public Arg<bool> DefineSwitch(string name, string shortName = null)
        {
            Func<string> container = DefineCustomOption(name, shortName, value => value);

            return new Arg<bool>(() =>
            {
                string value = container();

                return value != null && (value.Length == 0 || value.Equals("true", StringComparison.OrdinalIgnoreCase));
            });
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

            _processorsByFullName.Add(name, processor);

            if (!string.IsNullOrEmpty(shortName)) _processorsByShortName.Add(shortName, processor);

            return () =>
            {
                if (!ready) {
                    throw new InvalidOperationException("Value is undefined until Execute is called on the command.");
                }

                return value;
            };
        }

        internal void BuildCommand(string[] args) // ArraySlice<string>?
        {
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

                        if (!_processorsByFullName.TryGetValue(fullName, out processor)) {
                            throw new InvalidOperationException($"Unable to find option with name '{fullName}'.");
                        }
                    }
                    else if (chunk[0].StartsWith("/"))
                    {
                        string fullName = chunk[0].Substring(1);

                        if (!_processorsByFullName.TryGetValue(fullName, out processor)) {
                            throw new InvalidOperationException($"Unable to find option with name '{fullName}'.");
                        }
                    }
                    else if (chunk[0].StartsWith("-"))
                    {
                        string shortName = chunk[0].Substring(1);

                        if (!_processorsByShortName.TryGetValue(shortName, out processor)) {
                            throw new InvalidOperationException($"Unable to find option with short name '{shortName}'.");
                        }
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

            HashSet<Action<string[]>> unusedProcessors = new HashSet<Action<string[]>>(_processorsByFullName.Values);

            unusedProcessors.UnionWith(_processorsByShortName.Values);
            unusedProcessors.ExceptWith(seenProcessors);

            foreach (Action<string[]> processor in unusedProcessors) {
                processor(null);
            }
        }
    }
}