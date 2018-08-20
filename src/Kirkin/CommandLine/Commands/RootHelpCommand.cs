using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kirkin.CommandLine.Commands
{
    /// <summary>
    /// Root-level app help command.
    /// </summary>
    internal sealed class RootHelpCommand : IHelpCommand
    {
        private readonly CommandLineParser Parser;

        /// <summary>
        /// Parsed arguments.
        /// </summary>
        public CommandArguments Arguments { get; }

        /// <summary>
        /// Returns the name of the command.
        /// </summary>
        public string Name
        {
            get
            {
                return "help";
            }
        }

        internal RootHelpCommand(CommandLineParser parser)
        {
            Parser = parser;
            Arguments = new CommandArguments(null);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
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

            if (Parser.ShowAppDetailsInHelp)
            {
                AssemblyName info = entryAssembly.GetName();
                string version = GetAttribute<AssemblyVersionAttribute>(entryAssembly)?.Version ?? info.Version.ToString();
                AssemblyDescriptionAttribute description = GetAttribute<AssemblyDescriptionAttribute>(entryAssembly);

                if (description != null)
                {
                    sb.AppendLine($"{description.Description} ({info.Name}) v{version}");
                }
                else
                {
                    sb.AppendLine($"{info.Name} v{version}");
                }
            }

            string executableName = Path.GetFileNameWithoutExtension(entryAssembly.Location);

            sb.AppendLine($"Usage: {executableName} <command> [<args>].");
            sb.AppendLine();

            IEnumerable<CommandDefinition> commandDefinitions = Parser.CommandDefinitions;
            Dictionary<string, string> dictionary = commandDefinitions.ToDictionary(d => d.Name, d => d.Help, Parser.StringEqualityComparer);

            TextFormatter.FormatAsTable(dictionary, sb);

            return sb.ToString();
        }

        private static T GetAttribute<T>(ICustomAttributeProvider assembly, bool inherit = false) 
            where T : Attribute 
        {
             return assembly
                 .GetCustomAttributes(typeof(T), inherit)
                 .OfType<T>()
                 .FirstOrDefault();
        }
    }
}