using System;

namespace Kirkin.Collections.Async
{
    /// <summary>
    /// Wraps the result of a TryTake operation.
    /// </summary>
    public struct TakeResult<T>
    {
        private readonly T _value;
        private readonly bool _success;

        /// <summary>
        /// True if TryTake was successful.
        /// </summary>
        public bool Success
        {
            get
            {
                return _success;
            }
        }

        /// <summary>
        /// Value returned by TryTake.
        /// </summary>
        public T Value
        {
            get
            {
                if (!_success) {
                    throw new InvalidOperationException("Value is undefined when Success is false.");
                }

                return _value;
            }
        }

        /// <summary>
        /// Creates a new <see cref="TakeResult{T}"/>
        /// with the given Value and Success = true.
        /// </summary>
        internal TakeResult(T value)
        {
            _value = value;
            _success = true;
        }
    }
}