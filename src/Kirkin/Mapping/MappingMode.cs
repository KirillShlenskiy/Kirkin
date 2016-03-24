namespace Kirkin.Mapping
{
    /// <summary>
    /// Defines how strict the mapper is in terms of mapping
    /// readable source properties to writeable target properties.
    /// </summary>
    public enum MappingMode
    {
        /// <summary>
        /// Exact mapping.
        /// Ensures that all readable source members are mapped.
        /// Ensures that all writeable target members are mapped.
        /// This is the default for all mapping operations.
        /// </summary>
        Strict,

        /// <summary>
        /// Ensures that all writeable target members are mapped.
        /// Allows unmapped readable source members.
        /// </summary>
        AllTargetMembers,

        /// <summary>
        /// Ensures that all readable source members are mapped.
        /// Allows unmapped writeable target members.
        /// </summary>
        AllSourceMembers,

        /// <summary>
        /// Partial match. Allows unmapped readable
        /// source members and writeable target members.
        /// </summary>
        Relaxed
    }
}