using System;
using System.Linq.Expressions;

using Kirkin.Linq.Expressions;

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
            Action<T> setter = MakeSetter(expr);

            return new ValueRef<T>(getter, setter);
        }

        internal static Action<T> MakeSetter<T>(Expression<Func<T>> expr)
        {
            ParameterExpression valueParam = Expression.Parameter(typeof(T), "value");

            return Expression
                .Lambda<Action<T>>(Expression.Assign(expr.Body, valueParam), valueParam)
                .Compile();
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

        internal ValueRef<TRef> Ref<TRef>(Expression<Func<T, TRef>> expression)
        {
            // Expression currying: replace parameter with constant (reduce to Func<TRef>).
            Expression<Func<TRef>> reducedGetterExpression = Expression.Lambda<Func<TRef>>(
                new SubstituteParameterVisitor(Expression.Constant(Value)).Visit(expression.Body)
            );

            Func<TRef> getter = reducedGetterExpression.Compile();
            Action<TRef> setter;

            if (typeof(T).IsValueType)
            {
                // Value type: invoke getter, apply action, invoke setter.
                // T obj = Getter();
                // obj.Child = value;
                // Setter(obj);
                ParameterExpression value = Expression.Parameter(typeof(TRef), "value");
                ParameterExpression obj = Expression.Variable(typeof(T), "obj");

                Expression valuePropertyExpr = Expression.MakeMemberAccess(
                    Expression.Constant(this),
                    ExpressionUtil.Property<ValueRef<T>>(r => r.Value)
                );

                BlockExpression block = Expression.Block(
                    new[] { obj },
                    Expression.Assign(obj, valuePropertyExpr),
                    Expression.Assign(
                        new SubstituteParameterVisitor(obj).Visit(expression.Body),
                        value
                    ),
                    Expression.Assign(valuePropertyExpr, obj)
                );

                setter = Expression
                    .Lambda<Action<TRef>>(block, value)
                    .Compile();
            }
            else
            {
                // Simple case: reference type.
                setter = ValueRef.MakeSetter(reducedGetterExpression);
            }

            return new ValueRef<TRef>(getter, setter);
        }

        sealed class SubstituteParameterVisitor : ExpressionVisitor
        {
            private readonly Expression Replacement;

            internal SubstituteParameterVisitor(Expression replacement)
            {
                Replacement = replacement;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return Replacement;
            }
        }

        internal void Adjust(Func<T, T> func)
        {
            T value = Value;

            Value = func(value);
        }
    }
}