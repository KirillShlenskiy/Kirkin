namespace Kirkin.CommandLine
{
    public interface ICommand
    {
        string Name { get; }

        void Execute();
    }
}