using System;
using System.Linq.Expressions;

namespace Kirkin.Refs
{
    /// <summary>
    /// <see cref="ValueRef{T}"/> factory methods.
    /// </summary>
    public static class ValueRef
    {
        /// <summary>
        /// Creates a <see cref="ValueRef{T}"/> from the given expression.
        /// Throws if the expression cannot act as LHS in an assignment operation.
        /// </summary>
        public static ValueRef<T> FromExpression<T>(Expression<Func<T>> expr)
        {
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            Func<T> getter = expr.Compile();
            ParameterExpression valueParam = Expression.Parameter(typeof(T), "value");

            Action<T> setter = Expression
                .Lambda<Action<T>>(Expression.Assign(expr.Body, valueParam), valueParam)
                .Compile();

            return new ValueRef<T>(getter, setter);
        }
    }

    /// <summary>
    /// Reference to a value.
    /// </summary>
    /// <remarks>
    /// Provides both getter and setter on <see cref="Value"/> as read-only and write-only cases are already 
    /// adequately handled by <see cref="System.Func{T}"/> and <see cref="System.Action{T}"/> respectively.
    /// </remarks>
    public sealed class ValueRef<T> : IRef
    {
        private readonly Func<T> Getter;
        private readonly Action<T> Setter;

        /// <summary>
        /// Gets or sets the value that this reference is pointing to.
        /// </summary>
        public T Value
        {
            get
            {
                return Getter();
            }
            set
            {
                Setter(value);
            }
        }

        /// <summary>
        /// Gets or sets the value that this reference is pointing to.
        /// </summary>
        object IRef.Value
        {
            get
            {
                return Value;
            }
            set
            {
                Value = (T)value;
            }
        }

        /// <summary>
        /// Creates a new <see cref="ValueRef{T}"/> instance with the given value getter and setter.
        /// </summary>
        public ValueRef(Func<T> getter, Action<T> setter)
        {
            Getter = getter;
            Setter = setter;
        }

        internal void Adjust(Func<T, T> func)
        {
            T value = Value;

            Value = func(value);
        }
    }
}