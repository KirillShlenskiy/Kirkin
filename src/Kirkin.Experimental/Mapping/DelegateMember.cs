using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Kirkin.Mapping
{
    internal sealed class DelegateMember<TObject, TValue>
        : Member<TObject>
    {
        private readonly Func<TObject, TValue> _getter;
        private readonly Action<TObject, TValue> _setter;

        public override bool CanRead
        {
            get
            {
                return _getter != null;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return _setter != null;
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
            _getter = getter;
        }

        public DelegateMember(string name, Action<TObject, TValue> setter)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (setter == null) throw new ArgumentNullException(nameof(setter));

            Name = name;
            _setter = setter;
        }

        public DelegateMember(string name, Func<TObject, TValue> getter, Action<TObject, TValue> setter)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (getter == null) throw new ArgumentNullException(nameof(getter));
            if (setter == null) throw new ArgumentNullException(nameof(setter));

            Name = name;
            _getter = getter;
            _setter = setter;
        }

        protected internal override Expression ResolveGetter(ParameterExpression source)
        {
            if (!CanRead){
                throw new NotSupportedException($"This {nameof(DelegateMember<TObject, TValue>)} does not provide a getter.");
            }

            MethodInfo invokeMethod = typeof(Func<TObject, TValue>).GetMethod("Invoke");

            return Expression.Call(Expression.Constant(_getter), invokeMethod, source);
        }

        protected internal override Expression ResolveSetter(ParameterExpression target)
        {
            if (!CanWrite){
                throw new NotSupportedException($"This {nameof(DelegateMember<TObject, TValue>)} does not provide a setter.");
            }

            MethodInfo invokeMethod = typeof(Action<TObject, TValue>).GetMethod("Invoke");

            return Expression.Call(Expression.Constant(_getter), invokeMethod, target);
        }
    }
}