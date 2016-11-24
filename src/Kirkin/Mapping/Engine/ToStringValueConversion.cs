using System;
using System.Linq.Expressions;

namespace Kirkin.Mapping.Engine
{
    internal sealed class ToStringValueConversion : IValueConversion
    {
        public bool TryConvert(Expression value, Type targetType, NullableBehaviour nullableBehaviour, out Expression result)
        {
            if (targetType == typeof(string))
            {
                result = ToStringCall(value);

                return true;
            }

            result = null;

            return false;
        }

        private static Expression ToStringCall(Expression value)
        {
            Expression toStringExpr = Expression.Call(value, typeof(object).GetMethod(nameof(ToString)));

            // Call ToString directly on structs - except for
            // Nullable<T> which is treated as a reference type.
            if (value.Type.IsValueType && !(value.Type.IsGenericType && value.Type.GetGenericTypeDefinition() == typeof(Nullable<>))) {
                return toStringExpr;
            }

            ParameterExpression str = Expression.Parameter(typeof(string), nameof(str));

            return Expression.Block(
                new[] { str },
                Expression.IfThen(
                    Expression.NotEqual(value, ExpressionConstants.NullConstant),
                    Expression.Assign(str, toStringExpr)
                ),
                str
            );
        }
    }
}