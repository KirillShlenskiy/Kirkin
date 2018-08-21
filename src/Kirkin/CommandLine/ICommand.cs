namespace Kirkin.CommandLine
{
    /// <summary>
    /// Parsed console command.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Name of the command.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Parsed argument values.
        /// </summary>
        CommandArguments Args { get; }

        /// <summary>
        /// Populates any related parameter values and executes the command.
        /// </summary>
        void Execute();
    }
}