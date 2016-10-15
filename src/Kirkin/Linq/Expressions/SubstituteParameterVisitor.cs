using System.Linq.Expressions;

namespace Kirkin.Linq.Expressions
{
    /// <summary>
    /// <see cref="ExpressionVisitor"/> which substitutes all <see cref="ParameterExpression"/>
    /// instances found in the expression with the given parameter.
    /// </summary>
    internal sealed class SubstituteParameterVisitor : ExpressionVisitor
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