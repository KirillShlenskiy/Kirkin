using System;
using System.Collections.Generic;

namespace Kirkin.CommandLine.Commands
{
    internal sealed class HelpCommand : ICommand
    {
        private readonly CommandLineParser Parser;

        public string Name
        {
            get
            {
                return "help";
            }
        }

        internal HelpCommand(CommandLineParser parser)
        {
            Parser = parser;
        }

        public IDictionary<string, object> Arguments { get; } = new Dictionary<string, object>();

        public void Execute()
        {
            Console.WriteLine(Parser.ToString());
        }
    }
}