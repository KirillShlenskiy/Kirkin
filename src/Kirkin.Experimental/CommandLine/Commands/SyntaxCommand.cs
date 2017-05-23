using System;
using System.Collections.Generic;
using System.Linq;

namespace Kirkin.CommandLine.Commands
{
    /// <summary>
    /// Immutable facade over <see cref="CommandSyntax"/>.
    /// </summary>
    internal sealed class SyntaxCommand : ICommand
    {
        private readonly CommandSyntax _syntax;

        public string Name
        {
            get
            {
                return _syntax.Name;
            }
        }

        public IDictionary<string, object> Arguments
        {
            get
            {
                return _syntax.Arguments.ToDictionary(arg => arg.Name, arg => arg.Value, StringComparer.OrdinalIgnoreCase);
            }
        }

        internal SyntaxCommand(CommandSyntax syntax)
        {
            _syntax = syntax;
        }

        public void Execute()
        {
            _syntax.OnExecuted();
        }

        public override string ToString()
        {
            return _syntax.ToString();
        }
    }
}