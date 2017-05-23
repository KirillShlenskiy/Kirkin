using System;

namespace Kirkin.CommandLine
{
    internal sealed class ArgContainer<T>
    {
        internal bool Ready;
        internal bool _hasValue;
        internal T _value;

        public bool HasValue
        {
            get
            {
                ThrowIfNotReady();

                return _hasValue;
            }
        }

        public T Value
        {
            get
            {
                ThrowIfNotReady();

                if (!_hasValue) {
                    throw new InvalidOperationException($"Value is undefined when {nameof(HasValue)} is false.");
                }

                return _value;
            }
        }

        internal ArgContainer()
        {
        }

        public T GetValueOrDefault()
        {
            ThrowIfNotReady();

            return _hasValue ? _value : default(T);
        }

        public bool TryGetValue(out T value)
        {
            ThrowIfNotReady();

            if (_hasValue)
            {
                value = _value;

                return true;
            }

            value = default(T);

            return false;
        }

        private void ThrowIfNotReady()
        {
            if (!Ready) {
                throw new InvalidOperationException("The value of this instance is not yet ready.");
            }
        }
    }
}