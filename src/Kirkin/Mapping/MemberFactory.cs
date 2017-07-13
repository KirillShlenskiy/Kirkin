using System;
using System.Linq.Expressions;
using System.Reflection;
using Kirkin.Linq.Expressions;

namespace Kirkin.Mapping
{
    /// <summary>
    /// Type responsible for creating basic <see cref="Member{TObject}"/> instances.
    /// </summary>
    public class MemberFactory<T>
    {
        /// <summary>
        /// Creates a read-only <see cref="Member{TObject}"/> with the given name and getter expression.
        /// </summary>
        public virtual Member<T> ReadOnlyMember<TValue>(string name, Expression<Func<T, TValue>> getter)
        {
            return new HybridMember<TValue>(name, getter, null);
        }

        /// <summary>
        /// Creates a read-write <see cref="Member{TObject}"/> with the given name, getter expression and setter delegate.
        /// </summary>
        public virtual Member<T> ReadWriteMember<TValue>(string name, Expression<Func<T, TValue>> getter, Action<T, TValue> setter)
        {
            return new HybridMember<TValue>(name, getter, setter);
        }

        /// <summary>
        /// Creates a write-only <see cref="Member{TObject}"/> with the given name and setter delegate.
        /// </summary>
        public virtual Member<T> WriteOnlyMember<TValue>(string name, Action<T, TValue> setter)
        {
            return new HybridMember<TValue>(name, null, setter);
        }

        /// <summary>
        /// <see cref="Member{T}"/> implementation with an expression getter and a delegate setter.
        /// </summary>
        internal sealed class HybridMember<TValue>
            : Member<T>
        {
            private readonly Expression<Func<T, TValue>> Getter;
            private readonly Action<T, TValue> Setter;

            public override bool CanRead
            {
                get
                {
                    return Getter != null;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return Setter != null;
                }
            }

            public override string Name { get; }

            public override Type MemberType
            {
                get
                {
                    return typeof(object);
                }
            }

            internal HybridMember(string name, Expression<Func<T, TValue>> getter, Action<T, TValue> setter)
            {
                if (name == null) throw new ArgumentNullException(nameof(name));

                Name = name;
                Getter = getter;
                Setter = setter;
            }

            protected internal override Expression ResolveGetter(ParameterExpression source)
            {
                if (Getter == null) {
                    throw new NotSupportedException($"Member '{Name}' does not provide a getter.");
                }

                return new SubstituteParameterVisitor(source).Visit(Getter.Body);
            }

            protected internal override Expression ResolveSetter(ParameterExpression target)
            {
                if (!CanWrite) {
                    throw new NotSupportedException($"Member '{Name}' does not provide a setter.");
                }

                // Challenge: the expression needs to be assignable, so we'll use a proxy type with a
                // write-only property that will invoke the Setter action when its value is assigned.
                ConstructorInfo assignerConstructor = typeof(Assigner).GetConstructor(new[] { typeof(T), typeof(Action<T, TValue>) });

                if (assignerConstructor == null) {
                    throw new InvalidOperationException("Assignment proxy constructor cannot be resolved.");
                }

                return Expression.Property(
                    Expression.New(assignerConstructor, target, Expression.Constant(Setter)),
                    nameof(Assigner.Value)
                );
            }

            struct Assigner
            {
                private readonly T Object;
                private readonly Action<T, TValue> Action;

                public Assigner(T obj, Action<T, TValue> action)
                {
                    Object = obj;
                    Action = action;
                }

                public TValue Value
                {
                    set
                    {
                        Action(Object, value);
                    }
                }
            }
        }
    }
}