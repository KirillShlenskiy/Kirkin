namespace Kirkin.CommandLine.Commands.Help
{
    internal interface IHelpCommand : ICommand
    {
        string RenderHelpText();
    }
}