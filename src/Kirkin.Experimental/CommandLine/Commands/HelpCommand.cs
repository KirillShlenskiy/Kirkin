using System;

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

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}