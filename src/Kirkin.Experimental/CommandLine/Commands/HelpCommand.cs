using System;
using System.Collections.Generic;

namespace Kirkin.CommandLine.Commands
{
    internal sealed class HelpCommand : ICommand
    {
        public string Name
        {
            get
            {
                return "help";
            }
        }

        public IDictionary<string, object> Arguments { get; } = new Dictionary<string, object>();

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}