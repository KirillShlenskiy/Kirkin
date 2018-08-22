using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kirkin.CommandLine.Help
{
    internal sealed class IndividualCommandDefinitionHelpCommand : IHelpCommand
    {
        private readonly CommandDefinition Definition;

        public CommandArguments Args { get; }

        public string Name
        {
            get
            {
                return "help";
            }
        }

        internal IndividualCommandDefinitionHelpCommand(CommandDefinition definition)
        {
            Definition = definition;
            Args = new CommandArguments(definition);
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

            if (!string.IsNullOrEmpty(parents)) {
                sb.Append($"{parents} ");
            }

            sb.AppendLine($"{Definition}.");
            sb.AppendLine();

            Dictionary<string, string> dictionary = Definition.Parameters.ToDictionary(
                p => (p as IParameterFormattable)?.ToLongString() ?? p.ToString(),
                p => p.Help
            );

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