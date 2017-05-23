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

        public ICommandArg[] Arguments
        {
            get
            {
                return _syntax.Arguments.ToArray();
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
    }
}