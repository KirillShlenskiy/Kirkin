using System.Collections.Generic;

namespace Kirkin.CommandLine.Commands
{
    /// <summary>
    /// Immutable facade over <see cref="CommandDefinition"/>.
    /// </summary>
    internal sealed class DefaultCommand : ICommand
    {
        private readonly CommandDefinition _definition;

        public string Name
        {
            get
            {
                return _definition.Name;
            }
        }

        public IDictionary<string, object> Arguments { get; }

        internal DefaultCommand(CommandDefinition definition, IDictionary<string, object> arguments)
        {
            _definition = definition;
            Arguments = arguments;
        }

        public void Execute()
        {
            _definition.OnExecuted(this, Arguments);
        }

        public override string ToString()
        {
            return _definition.ToString();
        }
    }
}