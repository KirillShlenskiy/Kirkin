using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kirkin.CommandLine.Commands
{
    internal sealed class GeneralHelpCommand : IHelpCommand
    {
        private readonly CommandLineParser Parser;

        public CommandArguments Arguments { get; }

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
            Arguments = new CommandArguments(null);
        }

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
            StringBuilder sb = new StringBuilder();
            Assembly entryAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            string executableName = Path.GetFileNameWithoutExtension(entryAssembly.Location);

            sb.AppendLine($"Usage: {executableName} <command> [<args>].");
            sb.AppendLine();

            IEnumerable<CommandDefinition> commandDefinitions = Parser.CommandDefinitions;
            Dictionary<string, string> dictionary = commandDefinitions.ToDictionary(d => d.Name, d => d.Help, Parser.StringEqualityComparer);

            TextFormatter.FormatAsTable(dictionary, sb);

            return sb.ToString();
        }
    }
}