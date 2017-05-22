using System;

namespace Kirkin.CommandLine.Commands
{
    internal sealed class DelegateCommand : ICommand
    {
        public string Name { get; }
        internal Action Action { get; }

        internal DelegateCommand(string name, Action action)
        {
            Name = name;
            Action = action;
        }

        public void Execute()
        {
            Action();
        }
    }
}