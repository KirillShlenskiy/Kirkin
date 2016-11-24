using System;
using System.Linq.Expressions;

namespace Kirkin.Mapping.Engine
{
    sealed class NullableValueConversion : IValueConversion
    {
        public bool TryConvert(Expression value, Type targetType, NullableBehaviour nullableBehaviour, out Expression result)
        {
            Type sourceType = value.Type;
            Type nullableSourceType = Nullable.GetUnderlyingType(sourceType);
            Type nullableTargetType = Nullable.GetUnderlyingType(targetType);

            if (nullableSourceType != null ^ nullableTargetType != null)
            {
                result = NullableConversion(value, sourceType, nullableSourceType, targetType, nullableTargetType, nullableBehaviour);

                return true;
            }

            result = null;

            return false;
        }

        internal static Expression NullableConversion(
            Expression value,
            Type sourceType,
            Type nullableSourceType,
            Type targetType,
            Type nullableTargetType,
            NullableBehaviour behaviour)
        {
            if (behaviour == NullableBehaviour.Error) {
                throw new MappingException("Nullable to non-nullable and reverse conversions not allowed.");
            }

            if (nullableSourceType != null)
            {
                if (nullableTargetType == null)
                {
                    Expression coalesce = Expression.Coalesce(value, Expression.Default(nullableSourceType));

                    return (nullableSourceType == targetType)
                        ? coalesce
                        : Expression.Convert(coalesce, targetType);
                }
            }
            else if (nullableTargetType != null)
            {
                ParameterExpression result = Expression.Parameter(targetType, nameof(result));

                return Expression.Block(
                    new[] { result },
                    Expression.IfThenElse(
                        Expression.Equal(value, Expression.Default(sourceType)),
                        Expression.Assign(
                            result,
                            behaviour == NullableBehaviour.DefaultMapsToNull
                                ? (Expression)Expression.Default(targetType)
                                : Expression.Convert(Expression.Default(nullableTargetType), targetType)
                        ),
                        Expression.Assign(result, Expression.Convert(value, targetType))
                    ),
                    result
                );
            }

            throw new MappingException($"The required type conversion (from {sourceType} to {targetType}) is not implemented.");
        }
    }
}