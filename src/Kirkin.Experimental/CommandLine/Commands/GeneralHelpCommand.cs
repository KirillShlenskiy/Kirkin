﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Kirkin.CommandLine.Commands
{
    internal sealed class GeneralHelpCommand : IHelpCommand
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
        string IHelpCommand.RenderHelpText()
        {
            return RenderHelpText();
        }

        private string RenderHelpText()
        {
            // TODO: usage: replmon <command> [<args>].

            IEnumerable<CommandDefinition> commandDefinitions = Parser.CommandDefinitions;
            Dictionary<string, string> dictionary = commandDefinitions.ToDictionary(d => d.Name, d => d.Help, Parser.StringEqualityComparer);

            return TextFormatter.FormatAsTable(dictionary);
        }
    }
}