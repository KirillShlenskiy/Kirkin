namespace Kirkin.CommandLine.Commands
{
    internal interface IHelpCommand : ICommand
    {
        string RenderHelpText();
    }
}