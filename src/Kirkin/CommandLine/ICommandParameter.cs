namespace Kirkin.CommandLine
{
    /// <summary>
    /// Command parameter definition.
    /// </summary>
    public interface ICommandParameter
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
        /// Returns true if this parameter/option supports multiple input values.
        /// </summary>
        bool SupportsMultipleValues { get; }

        /// <summary>
        /// Specifies whether this parameter can be resolved by its position in addition to its name.
        /// </summary>
        bool IsPositionalParameter { get; }

        /// <summary>
        /// Human-readable parameter description.
        /// </summary>
        string Help { get; }
    }
}