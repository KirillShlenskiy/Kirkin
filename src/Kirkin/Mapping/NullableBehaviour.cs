namespace Kirkin.Mapping
{
    /// <summary>
    /// Supported behaviour when mapping nullable to non-nullable members.
    /// </summary>
    public enum NullableBehaviour
    {
        /// <summary>
        /// When assigning non-nullable values to a nullable member,
        /// default values will be substituted with null.
        /// When assigning nullable values to a non-nullable member,
        /// null values will be substituted with default.
        /// </summary>
        DefaultMapsToNull,

        /// <summary>
        /// When assigning non-nullable value to a nullable
        /// member, default values will be preserved.
        /// When assigning nullable values to a non-nullable member,
        /// null values will be substituted with default.
        /// </summary>
        AssignDefaultAsIs,
        
        /// <summary>
        /// An exception will be thrown.
        /// </summary>
        Error
    }
}