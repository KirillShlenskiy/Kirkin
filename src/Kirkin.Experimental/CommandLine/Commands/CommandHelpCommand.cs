using System;
using System.Collections.Generic;
using System.Linq;

namespace Kirkin.CommandLine.Commands
{
    internal sealed class CommandHelpCommand : IHelpCommand
    {
        private readonly CommandDefinition Command;
        private readonly IEqualityComparer<string> StringEqualityComparer;

        public string Name
        {
            get
            {
                return "help";
            }
        }

        internal CommandHelpCommand(CommandDefinition command, IEqualityComparer<string> stringEqualityComparer)
        {
            Command = command;
            StringEqualityComparer = stringEqualityComparer;
        }

        public IDictionary<string, object> Arguments { get; } = new Dictionary<string, object>();

        public void Execute()
        {
            Console.WriteLine(RenderHelpText());
        }

        /// <summary>
        /// Builds the help string.
        /// </summary>
        string IHelpCommand.RenderHelpText()
        {
            return RenderHelpText();
        }

        private string RenderHelpText()
        {
            // TODO: usage: replmon <command> [<args>].

            Dictionary<string, string> dictionary = new[] { Command.Parameter }
                .Concat(Command.Options)
                .ToDictionary(p => p.ToString(), p => p.Help, StringEqualityComparer);

            return TextFormatter.FormatAsTable(dictionary);
        }
    }
}