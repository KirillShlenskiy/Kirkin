using System;
using System.Collections.Generic;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Parsed command argument value container.
    /// </summary>
    public sealed class CommandArguments
    {
        static readonly Dictionary<string, object> EmptyArgDictionary = new Dictionary<string, object>();
        private readonly CommandDefinition Command;

        /// <summary>
        /// All command arguments.
        /// </summary>
#if NET_40
        public IDictionary<string, object> All { get; }
#else
        public IReadOnlyDictionary<string, object> All { get; }
#endif

        /// <summary>
        /// Creates a new <see cref="CommandArguments"/> instance.
        /// </summary>
#if NET_40
        internal CommandArguments(CommandDefinition command, IDictionary<string, object> all = null)
#else
        internal CommandArguments(CommandDefinition command, IReadOnlyDictionary<string, object> all = null)
#endif
        {
            Command = command;
            All = all ?? EmptyArgDictionary;
        }

        /// <summary>
        /// Returns the command's main parameter value.
        /// </summary>
        public string GetParameter()
        {
            if (Command?.Parameter == null) {
                throw new InvalidOperationException("The command does not define a parameter.");
            }

            return (string)All[Command.Parameter.Name];
        }

        /// <summary>
        /// Returns the value of the given option.
        /// </summary>
        public string GetOption(string name)
        {
            return (string)All[name];
        }

        /// <summary>
        /// Returns the value of the given option list.
        /// </summary>
        public string[] GetOptionList(string name)
        {
            return (string[])All[name];
        }

        /// <summary>
        /// Returns the value of the given switch.
        /// </summary>
        public bool GetSwitch(string name)
        {
            return (bool)All[name];
        }
    }
}