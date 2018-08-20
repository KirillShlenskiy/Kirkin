using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kirkin.CommandLine.Commands
{
    internal sealed class CommandGroupHelpCommand : IHelpCommand
    {
        private readonly CommandGroupDefinition Definition;

        public CommandArguments Arguments { get; }

        public string Name
        {
            get
            {
                return "help";
            }
        }

        internal CommandGroupHelpCommand(CommandGroupDefinition definition)
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

            return $"Usage: {executableName} {Definition} <command>.";
        }
    }
}