namespace Kirkin.CommandLine
{
    /// <summary>
    /// Command parameter definition.
    /// </summary>
    internal interface ICommandParameter
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
        /// Parses the given arguments and converts them to an appropriate value.
        /// </summary>
        object ParseArgs(string[] args);
    }
}