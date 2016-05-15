using System;
using System.Linq.Expressions;

namespace Kirkin.Mapping.Engine.MemberMappings
{
    /// <summary>
    /// Custom mapping between source and target.
    /// </summary>
    internal sealed class ExpressionMemberMapping<TSource, TTarget, TValue> : MemberMapping<TSource, TTarget>
    {
        /// <summary>
        /// Custom source value resolution delegate.
        /// </summary>
        private readonly Expression<Func<TSource, TValue>> SourceValueSelector;

        /// <summary>
        /// Creates a new delegate-based mapping instance.
        /// </summary>
        internal ExpressionMemberMapping(Member targetMember, Expression<Func<TSource, TValue>> sourceValueSelector)
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
            Expression<Func<TSource, TValue>> selectorWithSubstitutedParameter
                = (Expression<Func<TSource, TValue>>)new SubstituteParameterVisitor(sourceParam).Visit(SourceValueSelector);

            return selectorWithSubstitutedParameter.Body;
        }

        /// <summary>
        /// Returns a string description of this instance.
        /// </summary>
        public override string ToString()
        {
            return $"target.{TargetMember.Name} = sourceValueSelector(source)";
        }

        /// <summary>
        /// <see cref="ExpressionVisitor"/> which substitutes all <see cref="ParameterExpression"/>
        /// instances found in the expression with the given parameter.
        /// </summary>
        sealed class SubstituteParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression NewParameter;

            internal SubstituteParameterVisitor(ParameterExpression newParameter)
            {
                NewParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return NewParameter;
            }
        }
    }
}