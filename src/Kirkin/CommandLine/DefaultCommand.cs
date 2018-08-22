namespace Kirkin.CommandLine
{
    /// <summary>
    /// Immutable facade over <see cref="IndividualCommandDefinition"/>.
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

        public CommandArguments Args { get; }

        internal DefaultCommand(CommandDefinition definition, CommandArguments arguments)
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