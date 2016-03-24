using System;

namespace Kirkin
{
    /// <summary>
    /// Contains key/value event data.
    /// </summary>
    [Serializable]
    public class KeyValueEventArgs<TKey, TValue> : EventArgs
    {
        /// <summary>
        /// Key used when the value was created.
        /// </summary>
        public TKey Key { get; }

        /// <summary>
        /// Value created from the given key.
        /// </summary>
        public TValue Value { get; }

        /// <summary>
        /// Creates a new instance of <see cref="KeyValueEventArgs{TKey, TValue}"/>.
        /// </summary>
        public KeyValueEventArgs(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}