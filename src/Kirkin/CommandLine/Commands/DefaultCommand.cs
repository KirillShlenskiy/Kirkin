﻿namespace Kirkin.CommandLine.Commands
{
    /// <summary>
    /// Immutable facade over <see cref="IndividualCommandDefinition"/>.
    /// </summary>
    internal sealed class DefaultCommand : ICommand
    {
        private readonly IndividualCommandDefinition _definition;

        public string Name
        {
            get
            {
                return _definition.Name;
            }
        }

        public CommandArguments Args { get; }

        internal DefaultCommand(IndividualCommandDefinition definition, CommandArguments arguments)
        {
            _definition = definition;
            Args = arguments;
        }

        public void Execute()
        {
            _definition.RaiseExecutedEvent(this, Args);
        }

        public override string ToString()
        {
            return _definition.ToString();
        }
    }
}