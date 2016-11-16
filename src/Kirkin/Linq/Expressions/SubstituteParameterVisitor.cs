using System.Linq.Expressions;

namespace Kirkin.Linq.Expressions
{
    /// <summary>
    /// <see cref="ExpressionVisitor"/> which substitutes all <see cref="ParameterExpression"/>
    /// instances found in the expression with the given parameter.
    /// </summary>
    internal sealed class SubstituteParameterVisitor : ExpressionVisitor
    {
        // Using Expression rather than ParameterExpression to
        // support constant value injection in place of parameters.
        private readonly Expression NewParameter;

        internal SubstituteParameterVisitor(Expression newParameter)
        {
            NewParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return NewParameter;
        }
    }
}