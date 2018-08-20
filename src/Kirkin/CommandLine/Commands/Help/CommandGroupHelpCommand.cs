using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kirkin.CommandLine.Commands.Help
{
    internal sealed class CommandGroupHelpCommand : IHelpCommand
    {
        private readonly GroupCommandDefinition Definition;

        public CommandArguments Arguments { get; }

        public string Name
        {
            get
            {
                return "help";
            }
        }

        internal CommandGroupHelpCommand(GroupCommandDefinition definition)
        {
            Definition = definition;
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

            sb.Append($"Usage: {executableName} ");

            CommandDefinition parent = Definition.Parent;

            while (parent != null)
            {
                sb.Append($"{parent.Name} ");

                parent = parent.Parent;
            }

            sb.AppendLine($"{Definition}.");
            sb.AppendLine();

            Dictionary<string, string> dictionary = Definition.CommandDefinitions.ToDictionary(d => d.Name, d => d.Help, Definition.StringEqualityComparer);

            TextFormatter.FormatAsTable(dictionary, sb);

            return sb.ToString();
        }
    }
}