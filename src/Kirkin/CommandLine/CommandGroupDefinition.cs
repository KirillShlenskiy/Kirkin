using System;

namespace Kirkin.CommandLine
{
    internal sealed class CommandGroupDefinition : ICommandDefinition, ICommandListBuilder
    {
        private readonly CommandLineParser Parser;

        public string Name { get; }
        public string Help { get; set; }

        internal CommandGroupDefinition(string name, bool caseInsensitive)
        {
            Name = name;
            Parser = new CommandLineParser { CaseInsensitive = caseInsensitive };
        }

        public void DefineCommand(string name, Action<CommandDefinition> configureAction)
        {
            Parser.DefineCommand(name, configureAction);
        }

        public void DefineCommandGroup(string name, Action<ICommandListBuilder> configureAction)
        {
            Parser.DefineCommandGroup(name, configureAction);
        }

        public ICommand Parse(string[] args)
        {
            return Parser.Parse(args);
        }
    }
}