namespace Kirkin.Caching
{
    /// <summary>
    /// Defines supported cache thread safety modes.
    /// </summary>
    public enum CacheThreadSafetyMode
    {
        /// <summary>
        /// Fully thread-safe cache: value generation and storing are both protected by locks.
        /// </summary>
        Full,

        /// <summary>
        /// Multiple threads are allowed to generate the value
        /// at the same time. Only the last one gets to set it.
        /// </summary>
        PublicationOnly
    }
}