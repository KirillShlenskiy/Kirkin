namespace Kirkin.CommandLine.Help
{
    internal interface IHelpCommand : ICommand
    {
        string RenderHelpText();
    }
}