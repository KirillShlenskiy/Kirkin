using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Kirkin.Mapping
{
    /// <summary>
    /// Delegate-based <see cref="Member{T}"/> factory methods.
    /// </summary>
    public static class DelegateMember
    {
        public static Member<TObject> ReadOnly<TObject, TValue>(string name, Func<TObject, TValue> getter)
        {
            return new DelegateMember<TObject, TValue>(name, getter);
        }

        public static Member<TObject> WriteOnly<TObject, TValue>(string name, Action<TObject, TValue> setter)
        {
            return new DelegateMember<TObject, TValue>(name, setter);
        }

        public static Member<TObject> ReadWrite<TObject, TValue>(string name, Func<TObject, TValue> getter, Action<TObject, TValue> setter)
        {
            return new DelegateMember<TObject, TValue>(name, getter, setter);
        }
    }

    internal sealed class DelegateMember<TObject, TValue>
        : Member<TObject>
    {
        private readonly Func<TObject, TValue> Getter;
        private readonly Action<TObject, TValue> Setter;

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

        public override Type Type
        {
            get
            {
                return typeof(TValue);
            }
        }

        public DelegateMember(string name, Func<TObject, TValue> getter)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (getter == null) throw new ArgumentNullException(nameof(getter));

            Name = name;
            Getter = getter;
        }

        public DelegateMember(string name, Action<TObject, TValue> setter)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (setter == null) throw new ArgumentNullException(nameof(setter));

            Name = name;
            Setter = setter;
        }

        public DelegateMember(string name, Func<TObject, TValue> getter, Action<TObject, TValue> setter)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (getter == null) throw new ArgumentNullException(nameof(getter));
            if (setter == null) throw new ArgumentNullException(nameof(setter));

            Name = name;
            Getter = getter;
            Setter = setter;
        }

        protected internal override Expression ResolveGetter(ParameterExpression source)
        {
            if (!CanRead) {
                throw new NotSupportedException($"This {nameof(DelegateMember<TObject, TValue>)} does not provide a getter.");
            }

            MethodInfo invokeMethod = typeof(Func<TObject, TValue>).GetMethod("Invoke");
            
            return Expression.Call(Expression.Constant(Getter), invokeMethod, source);
        }

        protected internal override Expression ResolveSetter(ParameterExpression target)
        {
            if (!CanWrite) {
                throw new NotSupportedException($"This {nameof(DelegateMember<TObject, TValue>)} does not provide a setter.");
            }

            // Challenge: the expression needs to be assignable, so we'll use a proxy type with a
            // write-only property that will invoke the Setter action when its value is assigned.
            ConstructorInfo assignerConstructor = typeof(Assigner).GetConstructor(new[] { typeof(TObject), typeof(Action<TObject, TValue>) });

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
            private readonly TObject Object;
            private readonly Action<TObject, TValue> Action;

            public Assigner(TObject obj, Action<TObject, TValue> action)
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