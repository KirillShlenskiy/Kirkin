namespace Kirkin.CommandLine.CommandDefinitions
{
    /// <summary>
    /// Basic command defintion.
    /// </summary>
    public interface ICommandDefinition
    {
        /// <summary>
        /// The name of the command being configured.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Human-readable command description.
        /// </summary>
        string Help { get; set; }

        /// <summary>
        /// Parses the given args collection and produces a ready-to-use <see cref="ICommand"/> instance.
        /// </summary>
        ICommand Parse(string[] args);
    }
}