using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Kirkin.CommandLine.Parameters;

namespace Kirkin.CommandLine.Help
{
    internal sealed class CommandDefinitionHelpCommand : IHelpCommand
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

        internal CommandDefinitionHelpCommand(CommandDefinition definition)
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

            sb.Append($"{Definition}.");

            Dictionary<string, string> paramDictionary = Definition.Parameters.ToDictionary(
                p => (p as IParameterFormattable)?.ToLongString() ?? p.ToString(),
                p => p.Help
            );

            Dictionary<string, string> subCommandDictionary = Definition.SubCommands.ToDictionary(d => d.Name, d => d.Help, Definition.StringEqualityComparer);

            if (paramDictionary.Count != 0 || subCommandDictionary.Count != 0) {
                sb.AppendLine();
            }

            if (paramDictionary.Count != 0)
            {
                sb.AppendLine();

                TextFormatter.FormatAsTable(paramDictionary, sb);
            }

            if (subCommandDictionary.Count != 0)
            {
                sb.AppendLine();

                TextFormatter.FormatAsTable(subCommandDictionary, sb);
            }

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