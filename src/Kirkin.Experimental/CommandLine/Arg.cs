using System;

namespace Kirkin.CommandLine
{
    public interface IArg
    {
        bool HasValue { get; }
        object Value { get; }
    }

    public sealed class Arg<T> : IArg
    {
        internal bool Ready;
        internal bool _hasValue;
        internal T _value;

        internal ref T ValueRef
        {
            get
            {
                return ref _value;
            }
        }

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

        object IArg.Value
        {
            get
            {
                return Value;
            }
        }

        internal Arg()
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