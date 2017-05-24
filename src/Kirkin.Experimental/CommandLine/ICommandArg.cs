namespace Kirkin.CommandLine
{
    /// <summary>
    /// Command line argument whose value is only available once
    /// <see cref="CommandLineParser.Parse(string[])"/> has been called.
    /// </summary>
    internal interface ICommandArg
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
        object GetValue(string[] args);
    }
}