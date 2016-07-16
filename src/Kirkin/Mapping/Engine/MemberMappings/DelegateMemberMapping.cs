using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Kirkin.Mapping.Engine.MemberMappings
{
    /// <summary>
    /// Custom mapping between source and target.
    /// </summary>
    internal sealed class DelegateMemberMapping<TSource, TTarget, TValue>
        : MemberMapping<TSource, TTarget>
    {
        /// <summary>
        /// Custom source value resolution delegate.
        /// </summary>
        private readonly Func<TSource, TValue> SourceValueSelector;

        /// <summary>
        /// Creates a new delegate-based mapping instance.
        /// </summary>
        internal DelegateMemberMapping(Member<TTarget> targetMember, Func<TSource, TValue> sourceValueSelector)
            : base(targetMember)
        {
            SourceValueSelector = sourceValueSelector;
        }

        /// <summary>
        /// Produces an expression which resolves the source value by
        /// invoking the delegate specified when this instance was created.
        /// </summary>
        internal override Expression GetSourceValueExpression(ParameterExpression sourceParam)
        {
            MethodInfo invokeMethod = typeof(Func<TSource, TValue>).GetMethod("Invoke");
            Expression call = Expression.Call(Expression.Constant(SourceValueSelector), invokeMethod, sourceParam);

            return call;
        }

        /// <summary>
        /// Returns a string description of this instance.
        /// </summary>
        public override string ToString()
        {
            return $"target.{TargetMember.Name} = sourceValueSelector(source)";
        }
    }
}