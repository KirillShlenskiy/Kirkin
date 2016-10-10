using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Kirkin.Mapping
{
    internal sealed class DelegateMember<TObject, TValue>
        : Member<TObject>
    {
        private readonly Expression<Func<TObject, TValue>> _getter;
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

        public DelegateMember(string name, Expression<Func<TObject, TValue>> getter)
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

        public DelegateMember(string name, Expression<Func<TObject, TValue>> getter, Action<TObject, TValue> setter)
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
            if (!CanRead) {
                throw new NotSupportedException($"This {nameof(DelegateMember<TObject, TValue>)} does not provide a getter.");
            }

            return new SubstituteParameterVisitor(source).Visit(_getter.Body);
        }

        protected internal override Expression ResolveSetter(ParameterExpression target)
        {
            throw new NotImplementedException();

            //if (!CanWrite) {
            //    throw new NotSupportedException($"This {nameof(ExpressionMember<TObject, TValue>)} does not provide a setter.");
            //}

            //MethodInfo invokeMethod = typeof(Action<TObject, TValue>).GetMethod("Invoke");

            //return Expression.Call(Expression.Constant(_setter), invokeMethod, target);
        }

        sealed class SubstituteParameterVisitor : ExpressionVisitor
        {
            public readonly ParameterExpression NewParameterExpression;

            internal SubstituteParameterVisitor(ParameterExpression newParameterExpression)
            {
                NewParameterExpression = newParameterExpression;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return NewParameterExpression;
            }
        }
    }
}