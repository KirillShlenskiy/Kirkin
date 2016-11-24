using System.Linq.Expressions;

namespace Kirkin.Mapping.Engine
{
    internal static class ExpressionConstants
    {
        public static readonly ConstantExpression NullConstant = Expression.Constant(null);
    }
}