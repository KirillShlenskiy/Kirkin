using System;

using Kirkin.CommandLine.Commands;

namespace Kirkin.CommandLine
{
    /// <summary>
    /// Command definition.
    /// </summary>
    public abstract class CommandDefinitionBase
    {
        /// <summary>
        /// The name of the command being configured.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Human-readable command description.
        /// </summary>
        public string Help { get; set; }

        /// <summary>
        /// Parent command specified when this instance was created.
        /// </summary>
        internal CommandDefinitionBase Parent { get; }

        /// <summary>
        /// Creates a new <see cref="CommandDefinitionBase"/> instance.
        /// </summary>
        private protected CommandDefinitionBase(string name, CommandDefinitionBase parent)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            Name = name;
            Parent = parent;
        }

        /// <summary>
        /// Parses the given args collection and produces a ready-to-use <see cref="ICommand"/> instance.
        /// </summary>
        public abstract ICommand Parse(string[] args);

        private protected abstract IHelpCommand CreateHelpCommand();
    }
}