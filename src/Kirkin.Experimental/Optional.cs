using System;

namespace Kirkin
{
    /// <summary>
    /// Simple optional value holder.
    /// </summary>
    public struct Optional<T>
    {
        private readonly bool _hasValue;
        private readonly T _value;

        /// <summary>
        /// True if the value was specified. Otherwise false.
        /// </summary>
        public bool HasValue
        {
            get
            {
                return _hasValue;
            }
        }

        /// <summary>
        /// Value which was specified when this object was created.
        /// </summary>
        public T Value
        {
            get
            {
                if (!_hasValue) {
                    throw new InvalidOperationException("Value is undefined when HasValue is false.");
                }

                return _value;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="Optional{T}"/> with the given value.
        /// Regardless of what the value is, HasValue will be true for this instance.
        /// </summary>
        public Optional(T value)
        {
            _value = value;
            _hasValue = true;
        }

        /// <summary>
        /// Returns the value of the current <see cref="Optional{T}"/> object, or the object's default value.
        /// </summary>
        public T GetValueOrDefault()
        {
            return _value;
        }

        /// <summary>
        /// Returns the value of the current <see cref="Optional{T}"/> object, or the specified default value.
        /// </summary>
        public T GetValueOrDefault(T defaultValue)
        {
            return _hasValue ? _value : defaultValue;
        }

        /// <summary>
        /// Returns the text representation of the value of the current <see cref="Optional{T}"/> object.
        /// </summary>
        public override string ToString()
        {
            return _hasValue ? _value.ToString() : "";
        }
    }
}