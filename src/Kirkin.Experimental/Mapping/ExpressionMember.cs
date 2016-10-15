using System;
using System.Linq.Expressions;

using Kirkin.Linq.Expressions;

namespace Kirkin.Mapping
{
    /// <summary>
    /// Read-only expression-based <see cref="Member{T}"/> implementation.
    /// </summary>
    public sealed class ExpressionMember<TObject, TValue>
        : Member<TObject>
    {
        private readonly Expression<Func<TObject, TValue>> Getter;

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
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

        public ExpressionMember(string name, Expression<Func<TObject, TValue>> getter)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (getter == null) throw new ArgumentNullException(nameof(getter));

            Name = name;
            Getter = getter;
        }

        protected internal override Expression ResolveGetter(ParameterExpression source)
        {
            return new SubstituteParameterVisitor(source).Visit(Getter.Body);
        }

        protected internal override Expression ResolveSetter(ParameterExpression target)
        {
            throw new NotSupportedException($"This {nameof(ExpressionMember<TObject, TValue>)} does not provide a setter.");
        }
    }

    /// <summary>
    /// Read-only expression-based <see cref="Member{T}"/> implementation.
    /// </summary>
    internal sealed class ExpressionMember<TObject>
        : Member<TObject>
    {
        private readonly Expression<Func<TObject, object>> Getter;

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override string Name { get; }

        public override Type Type
        {
            get
            {
                return typeof(object);
            }
        }

        public ExpressionMember(string name, Expression<Func<TObject, object>> getter)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (getter == null) throw new ArgumentNullException(nameof(getter));

            Name = name;
            Getter = getter;
        }

        protected internal override Expression ResolveGetter(ParameterExpression source)
        {
            return new SubstituteParameterVisitor(source).Visit(Getter.Body);
        }

        protected internal override Expression ResolveSetter(ParameterExpression target)
        {
            throw new NotSupportedException($"This {nameof(ExpressionMember<TObject>)} does not provide a setter.");
        }
    }
}