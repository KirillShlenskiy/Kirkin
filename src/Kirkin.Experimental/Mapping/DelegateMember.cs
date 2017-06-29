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
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (getter == null) throw new ArgumentNullException(nameof(getter));

            return new DelegateMember<TObject, TValue>(name, getter, null);
        }

        public static Member<TObject> ReadOnly<TObject>(string name, Type memberType, Func<TObject, object> getter)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (memberType == null) throw new ArgumentNullException(nameof(memberType));
            if (getter == null) throw new ArgumentNullException(nameof(getter));

            return new DelegateMember<TObject>(name, memberType, getter, null);
        }

        public static Member<TObject> WriteOnly<TObject, TValue>(string name, Action<TObject, TValue> setter)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (setter == null) throw new ArgumentNullException(nameof(setter));

            return new DelegateMember<TObject, TValue>(name, null, setter);
        }

        public static Member<TObject> WriteOnly<TObject>(string name, Type memberType, Action<TObject, object> setter)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (memberType == null) throw new ArgumentNullException(nameof(memberType));
            if (setter == null) throw new ArgumentNullException(nameof(setter));

            return new DelegateMember<TObject>(name, memberType, null, setter);
        }

        public static Member<TObject> ReadWrite<TObject, TValue>(string name, Func<TObject, TValue> getter, Action<TObject, TValue> setter)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (getter == null) throw new ArgumentNullException(nameof(getter));
            if (setter == null) throw new ArgumentNullException(nameof(setter));

            return new DelegateMember<TObject, TValue>(name, getter, setter);
        }

        public static Member<TObject> ReadWrite<TObject>(string name, Type memberType, Func<TObject, object> getter, Action<TObject, object> setter)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (memberType == null) throw new ArgumentNullException(nameof(memberType));
            if (getter == null) throw new ArgumentNullException(nameof(getter));
            if (setter == null) throw new ArgumentNullException(nameof(setter));

            return new DelegateMember<TObject>(name, memberType, getter, setter);
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

        public override Type MemberType
        {
            get
            {
                return typeof(TValue);
            }
        }

        internal DelegateMember(string name, Func<TObject, TValue> getter, Action<TObject, TValue> setter)
        {
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
            private readonly Action<TObject, TValue> Action;
            private readonly TObject Object;

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

    public sealed class DelegateMember<TObject>
        : Member<TObject>
    {
        public static Member<TObject> ReadOnly<TValue>(string name, Func<TObject, TValue> getter)
        {
            return DelegateMember.ReadOnly(name, getter);
        }

        public static Member<TObject> WriteOnly<TValue>(string name, Action<TObject, TValue> setter)
        {
            return DelegateMember.WriteOnly(name, setter);
        }

        public static Member<TObject> ReadWrite<TValue>(string name, Func<TObject, TValue> getter, Action<TObject, TValue> setter)
        {
            return DelegateMember.ReadWrite(name, getter, setter);
        }

        private readonly Func<TObject, object> Getter;
        private readonly Action<TObject, object> Setter;

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
        public override Type MemberType { get; }

        internal DelegateMember(string name, Type memberType, Func<TObject, object> getter, Action<TObject, object> setter)
        {
            Name = name;
            MemberType = memberType;
            Getter = getter;
            Setter = setter;
        }

        protected internal override Expression ResolveGetter(ParameterExpression source)
        {
            if (!CanRead) {
                throw new NotSupportedException($"This {nameof(DelegateMember<TObject>)} does not provide a getter.");
            }

            MethodInfo invokeMethod = typeof(Func<TObject, object>).GetMethod("Invoke");
            
            return Expression.Convert(
                Expression.Call(Expression.Constant(Getter), invokeMethod, source), MemberType
            );
        }

        protected internal override Expression ResolveSetter(ParameterExpression target)
        {
            if (!CanWrite) {
                throw new NotSupportedException($"This {nameof(DelegateMember<TObject>)} does not provide a setter.");
            }

            // Challenge: the expression needs to be assignable, so we'll use a proxy type with a
            // write-only property that will invoke the Setter action when its value is assigned.
            ConstructorInfo assignerConstructor = typeof(Assigner<>)
                .MakeGenericType(typeof(TObject), MemberType)
                .GetConstructor(new[] { typeof(TObject), typeof(Action<TObject, object>) });

            if (assignerConstructor == null) {
                throw new InvalidOperationException("Assignment proxy constructor cannot be resolved.");
            }

            return Expression.Property(
                Expression.New(assignerConstructor, target, Expression.Constant(Setter)),
                nameof(Assigner<object>.Value)
            );
        }

        struct Assigner<TValue>
        {
            private readonly Action<TObject, object> Action;
            private readonly TObject Object;

            public Assigner(TObject obj, Action<TObject, object> action)
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