using System;
using System.Linq.Expressions;

namespace Kirkin.Mapping.Engine
{
    internal sealed class StringToEnumValueConversion : IValueConversion
    {
        public bool TryConvert(Expression value, Type targetType, NullableBehaviour nullableBehaviour, out Expression result)
        {
            Type sourceType = value.Type;
            Type nullableTargetType = Nullable.GetUnderlyingType(targetType);

            if (sourceType == typeof(string) && (nullableTargetType ?? targetType).IsEnum)
            {
                result = StringToEnumConversion(value, targetType, nullableTargetType, nullableBehaviour);

                return true;
            }

            result = null;

            return false;
        }

        private static Expression StringToEnumConversion(Expression value, Type targetType, Type nullableTargetType, NullableBehaviour behaviour)
        {
            ParameterExpression result = Expression.Parameter(targetType, nameof(result)); // Enum or Nullable<Enum>.

            return Expression.Block(
                new[] { result },
                Expression.IfThenElse(
                    Expression.Equal(value, ExpressionConstants.NullConstant),
                    (nullableTargetType == null && behaviour == NullableBehaviour.Error)
                        ? (Expression)Expression.Throw(Expression.Constant(new MappingException("Null string to nun-nullable Enum not supported.")))
                        : Expression.Assign(result, Expression.Default(targetType)),
                    Expression.Assign(
                        result,
                        Expression.Convert(
                            Expression.Call(
                                typeof(Enum).GetMethod(nameof(Enum.Parse),
                                new[] { typeof(Type), typeof(string), typeof(bool) }),
                                Expression.Constant(nullableTargetType ?? targetType),
                                value,
                                Expression.Constant(true)
                            ),
                            targetType
                        )
                    )
                ),
                result
            );
        }
    }
}