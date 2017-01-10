using System;

namespace Kirkin.Refs
{
    internal sealed class ObservableRef<T> : Ref<T>
    {
        private T _value;

        public ObservableRef()
        {
        }

        public ObservableRef(T initialValue)
        {
            _value = initialValue;
        }

        public override T Value
        {
            get
            {
                return _value;
            }
            set
            {
                T oldValue = _value;
                _value = value;

                ValueSet?.Invoke(this, EventArgs.Empty);

                if (!Equals(value, oldValue)) {
                    ValueChanged?.Invoke(this, new ValueChangedEventArgs<T>(oldValue, value));
                }
            }
        }

        public event EventHandler<ValueChangedEventArgs<T>> ValueChanged;
        public event EventHandler ValueSet;
    }

    public sealed class ValueChangedEventArgs<T> : EventArgs
    {
        public T OldValue { get; }
        public T NewValue { get; }

        internal ValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
