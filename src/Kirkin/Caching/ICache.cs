namespace Kirkin.Caching
{
    /// <summary>
    /// Common cache interface.
    /// </summary>
    public interface ICache<out T>
    {
        /// <summary>
        /// Returns the cached value initialising it if necessary.
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Returns true if the cached value is current and ready to use.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Invalidates the cache causing it to be rebuilt.
        /// </summary>
        void Invalidate();
    }
}