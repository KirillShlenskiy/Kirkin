using System;
using System.Collections.Generic;
using System.Linq;

namespace Kirkin.CommandLine.Commands
{
    internal sealed class GeneralHelpCommand : ICommand
    {
        private readonly CommandLineParser Parser;

        public string Name
        {
            get
            {
                return "help";
            }
        }

        internal GeneralHelpCommand(CommandLineParser parser)
        {
            Parser = parser;
        }

        public IDictionary<string, object> Arguments { get; } = new Dictionary<string, object>();

        public void Execute()
        {
            Console.WriteLine(RenderHelpText());
        }

        /// <summary>
        /// Builds the help string.
        /// </summary>
        internal string RenderHelpText()
        {
            IEnumerable<CommandDefinition> commandDefinitions = Parser.CommandDefinitions;
            Dictionary<string, string> dictionary = commandDefinitions.ToDictionary(d => d.Name, d => d.Help, Parser.StringEqualityComparer);

            return TextFormatter.FormatAsTable(dictionary);
        }
    }
}