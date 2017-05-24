using System;
using System.Collections.Generic;
using System.Linq;

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

        public IDictionary<string, object> Arguments
        {
            get
            {
                return _definition.Arguments.ToDictionary(arg => arg.Name, arg => arg.Value, StringComparer.OrdinalIgnoreCase);
            }
        }

        internal DefaultCommand(CommandDefinition definition)
        {
            _definition = definition;
        }

        public void Execute()
        {
            _definition.OnExecuted();
        }

        public override string ToString()
        {
            return _definition.ToString();
        }
    }
}