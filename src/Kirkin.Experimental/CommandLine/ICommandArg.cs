namespace Kirkin.CommandLine
{
    /// <summary>
    /// Parsed command line argument whose value is only available
    /// once <see cref="ICommand.Execute"/> has been called.
    /// </summary>
    public interface ICommandArg
    {
        /// <summary>
        /// Parameter name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Parameter short name (or null).
        /// </summary>
        string ShortName { get; }

        /// <summary>
        /// Parsed argument value. Only available once <see cref="ICommand.Execute"/> has been called.
        /// </summary>
        object Value { get; }
    }
}