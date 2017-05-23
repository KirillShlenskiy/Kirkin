using System;

using Kirkin.Collections.Generic;

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

        public ICommandArg[] Arguments => Array<ICommandArg>.Empty;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}