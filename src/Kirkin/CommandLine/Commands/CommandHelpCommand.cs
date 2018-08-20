using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kirkin.CommandLine.Commands
{
    internal sealed class CommandHelpCommand : IHelpCommand
    {
        private readonly CommandDefinition Definition;

        public CommandArguments Arguments { get; }

        public string Name
        {
            get
            {
                return "help";
            }
        }

        internal CommandHelpCommand(CommandDefinition definition)
        {
            Definition = definition;
            Arguments = new CommandArguments(definition);
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

            sb.AppendLine($"Usage: {executableName} {Definition}.");
            sb.AppendLine();

            List<ICommandParameter> parameters = new List<ICommandParameter>();

            if (Definition.Parameter != null) {
                parameters.Add(Definition.Parameter);
            }

            parameters.AddRange(Definition.Options);

            Dictionary<string, string> dictionary = parameters.ToDictionary(p => (p as IParameterFormattable)?.ToLongString() ?? p.ToString(), p => p.Help);

            TextFormatter.FormatAsTable(dictionary, sb);

            return sb.ToString();
        }
    }
}