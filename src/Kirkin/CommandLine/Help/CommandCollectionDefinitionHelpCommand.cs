using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kirkin.CommandLine.Help
{
    internal sealed class CommandCollectionDefinitionHelpCommand : IHelpCommand
    {
        private readonly CommandCollectionDefinition Definition;

        public CommandArguments Args { get; }

        public string Name
        {
            get
            {
                return "help";
            }
        }

        internal CommandCollectionDefinitionHelpCommand(CommandCollectionDefinition definition)
        {
            Definition = definition;
            Args = new CommandArguments(null);
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

            string parents = string.Join(" ", EnumerateParentNames().Reverse());

            if (!string.IsNullOrEmpty(parents))
            {
                sb.Append($"{parents} ");
            }

            sb.AppendLine($"{Definition}.");
            sb.AppendLine();

            Dictionary<string, string> dictionary = Definition.Commands.ToDictionary(d => d.Name, d => d.Help, Definition.StringEqualityComparer);

            TextFormatter.FormatAsTable(dictionary, sb);

            return sb.ToString();
        }

        private IEnumerable<string> EnumerateParentNames()
        {
            CommandDefinition parent = Definition.Parent;

            while (parent != null)
            {
                yield return parent.Name;

                parent = parent.Parent;
            }
        }
    }
}