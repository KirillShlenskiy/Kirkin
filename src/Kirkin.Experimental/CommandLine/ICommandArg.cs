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
        /// Parsed argument value. Only available once <see cref="CommandLineParser.Parse(string[])"/> has been called.
        /// </summary>
        object Value { get; }
    }
}