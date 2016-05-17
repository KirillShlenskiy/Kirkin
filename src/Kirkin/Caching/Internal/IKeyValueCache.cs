namespace Kirkin.Caching.Internal
{
    /// <summary>
    /// Contract for caches with key/value support.
    /// </summary>
    internal interface IKeyValueCache<TKey, TValue>
    {
        /// <summary>
        /// Returns the cached value appropriate for the given key initialising it if necessary.
        /// </summary>
        TValue GetValue(TKey key);

        /// <summary>
        /// Returns true if the cache contains a valid value for the given key.
        /// </summary>
        bool IsValid(TKey key);

        /// <summary>
        /// Invalidates the cache causing it to be rebuilt.
        /// </summary>
        void Invalidate(TKey key);
    }
}