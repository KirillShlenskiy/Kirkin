using System;
using System.Linq.Expressions;

namespace Kirkin.Mapping.Engine
{
    /// <summary>
    /// Type which orchestrates expression type conversions.
    /// </summary>
    internal static class ExpressionTypeConverter
    {
        /// <summary>
        /// Applies well-known type conversions to the given source
        /// expression so as to produce the desired return type.
        /// </summary>
        public static Expression ChangeType(Expression value, Type targetType, NullableBehaviour nullableBehaviour)
        {
            if (value.Type == targetType)
            {
                // Fast path: no conversion required.
                return value;
            }

            return ComplexConversion(value, targetType, nullableBehaviour);
        }

        private static Expression ComplexConversion(Expression value, Type targetType, NullableBehaviour nullableBehaviour)
        {
            // Nullable handling.
            Type sourceType = value.Type;
            Type nullableSourceType = Nullable.GetUnderlyingType(sourceType);
            Type nullableTargetType = Nullable.GetUnderlyingType(targetType);

            // String -> Enum mapping (taken from ExpressMapper and improved).
            // Must come before "normal" nullable/non-nullable conversions.
            if (sourceType == typeof(string) && (nullableTargetType ?? targetType).IsEnum) {
                return StringToEnumConversion(value, targetType, nullableTargetType, nullableBehaviour);
            }

            // Non-string -> string (simply calls value.ToString()).
            if (targetType == typeof(string)) {
                return ToStringCall(value);
            }

            // Nullable -> non-nullable or non-nullable -> nullable.
            if (nullableSourceType != null ^ nullableTargetType != null) {
                return NullableConversion(value, sourceType, nullableSourceType, targetType, nullableTargetType, nullableBehaviour);
            }

            // Attempt cast (will likely work for most IConvertible
            // types, so no special handling for them).
            return Expression.Convert(value, targetType);
        }

        #region Conversions

        static class ExpressionConstants
        {
            public static readonly ConstantExpression NullConstant = Expression.Constant(null);
        }

        private static Expression NullableConversion(
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

        #endregion
    }
}