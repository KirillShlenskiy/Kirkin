using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Kirkin.Linq.Expressions;

namespace Kirkin.Refs
{
    /// <summary>
    /// Mutable reference to a value (<see cref="ValueRef{T}"/>) factory methods.
    /// </summary>
    public static class ValueRef
    {
        /// <summary>
        /// Creates a <see cref="ValueRef{T}"/> from the given expression.
        /// Throws if the expression cannot act as LHS in an assignment operation.
        /// </summary>
        public static ValueRef<T> Assignable<T>(Expression<Func<T>> expr)
        {
            if (expr == null) throw new ArgumentNullException(nameof(expr));

            Func<T> getter = expr.Compile();
            Action<T> setter = MakeSetter(expr);

            return new ValueRef<T>(getter, setter);
        }

        /// <summary>
        /// Value assignment shortcut utility which performs all the necessary
        /// copies of struct values in the member chain along the way.
        /// </summary>
        internal static void Set<T>(Expression<Func<T>> assignableExpr, T value)
        {
            // Example:
            // ValueRef<int> widthRef = ValueRef
            //    .Assignable(() => dummy.Frame)
            //    .Ref(f => f.Size)
            //    .Ref(s => s.Width);
            //
            // widthRef.Value = 123;
            //
            // VS:
            //
            // ValueRef.Set(() => dummy.Frame.Size.Width, 123);
            throw new NotImplementedException();

            Stack<MemberExpression> memberExpressions = new Stack<MemberExpression>();
            Expression expr = assignableExpr.Body;
            MemberExpression memberExpr;

            while ((memberExpr = expr as MemberExpression) != null)
            {
                memberExpressions.Push(memberExpr);

                expr = memberExpr;
            }
        }

        internal static Action<T> MakeSetter<T>(Expression<Func<T>> expr)
        {
            ParameterExpression valueParam = Expression.Parameter(typeof(T), "value");

            // We're not substituting parameter because there isn't one to substitute.
            // The container object is closed over (producing a ConstantExpression).
            return Expression
                .Lambda<Action<T>>(Expression.Assign(expr.Body, valueParam), valueParam)
                .Compile();
        }
    }

    /// <summary>
    /// Mutable reference to a value.
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
        internal ValueRef(Func<T> getter, Action<T> setter)
        {
            if (getter == null) throw new ArgumentNullException(nameof(getter));
            if (setter == null) throw new ArgumentNullException(nameof(setter));

            Getter = getter;
            Setter = setter;
        }

        /// <summary>
        /// Produces a reference to a child member of the value that this <see cref="ValueRef{T}"/> is pointing to.
        /// </summary>
        public ValueRef<TChild> Ref<TChild>(Expression<Func<T, TChild>> expression)
        {
            Expression valuePropertyExpr = Expression.MakeMemberAccess(
                Expression.Constant(this),
                typeof(ValueRef<T>).GetProperty(nameof(Value))
            );

            // Expression currying: replace parameter with Value property access (reducing it to Func<TChild>).
            Expression<Func<TChild>> reducedGetterExpression = Expression.Lambda<Func<TChild>>(
                new SubstituteParameterVisitor(valuePropertyExpr).Visit(expression.Body)
            );

            Func<TChild> getter = reducedGetterExpression.Compile();
            Action<TChild> setter;

            if (typeof(T).IsValueType)
            {
                // Value type: invoke getter, apply action, invoke setter.
                ParameterExpression value = Expression.Parameter(typeof(TChild), "value");
                ParameterExpression obj = Expression.Variable(typeof(T), "obj");

                // T obj;
                // obj = this.Value;
                // obj.Child = value;
                // this.Value = obj;
                BlockExpression block = Expression.Block(
                    new[] { obj },
                    Expression.Assign(obj, valuePropertyExpr),
                    Expression.Assign(new SubstituteParameterVisitor(obj).Visit(expression.Body), value),
                    Expression.Assign(valuePropertyExpr, obj)
                );

                setter = Expression
                    .Lambda<Action<TChild>>(block, value)
                    .Compile();
            }
            else
            {
                // Simple case: reference type.
                setter = ValueRef.MakeSetter(reducedGetterExpression);
            }

            return new ValueRef<TChild>(getter, setter);
        }

        internal void Adjust(Func<T, T> func)
        {
            T value = Value;

            Value = func(value);
        }
    }
}