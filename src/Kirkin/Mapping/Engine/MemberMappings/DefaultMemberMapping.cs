using System;
using System.Linq.Expressions;

using Kirkin.Utilities;

namespace Kirkin.Mapping.Engine.MemberMappings
{
    /// <summary>
    /// Direct mapping between source and target members with support for conversions.
    /// </summary>
    internal sealed class DefaultMemberMapping<TSource, TTarget> : MemberMapping<TSource, TTarget>
        , IEquatable<DefaultMemberMapping<TSource, TTarget>>
    {
        /// <summary>
        /// Mapping source member.
        /// </summary>
        public Member SourceMember { get; }

        /// <summary>
        /// Nullable to non-nullable conversion behaviour of this instance.
        /// </summary>
        public NullableBehaviour NullableBehaviour { get; }

        /// <summary>
        /// Creates a new simple member-to-member mapping instance.
        /// </summary>
        public DefaultMemberMapping(Member sourceMember, Member targetMember, NullableBehaviour nullableBehaviour)
            : base(targetMember)
        {
            SourceMember = sourceMember;
            NullableBehaviour = nullableBehaviour;
        }

        /// <summary>
        /// Produces an expression which resolves the source value.
        /// </summary>
        internal override Expression GetSourceValueExpression(ParameterExpression sourceParam)
        {
            // Resolve and convert value automatically.
            Expression source = SourceMember.ResolveGetter(sourceParam);

            if (SourceMember.Type != TargetMember.Type) {
                source = ComplexConversion(source, SourceMember.Type, TargetMember.Type);
            }

            return source;
        }

        /// <summary>
        /// Produces the expression which retrieves the source value from the appropriate member.
        /// </summary>
        private Expression ComplexConversion(Expression value, Type sourceType, Type targetType)
        {
            // Nullable handling.
            Type nullableSourceType = Nullable.GetUnderlyingType(sourceType);
            Type nullableTargetType = Nullable.GetUnderlyingType(targetType);

            // String -> Enum mapping (taken from ExpressMapper and improved).
            // Must come before "normal" nullable/non-nullable conversions.
            if (sourceType == typeof(string) && (nullableTargetType ?? targetType).IsEnum) {
                return StringToEnumConversion(value, targetType, nullableTargetType, NullableBehaviour);
            }

            // Non-string -> string (simply calls value.ToString()).
            if (targetType == typeof(string)) {
                return ToStringCall(value);
            }

            // Nullable -> non-nullable or non-nullable -> nullable.
            if (nullableSourceType != null ^ nullableTargetType != null) {
                return NullableConversion(value, sourceType, nullableSourceType, targetType, nullableTargetType, NullableBehaviour);
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
                ParameterExpression result = Expression.Parameter(targetType, "result");

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
            ParameterExpression result = Expression.Parameter(targetType, "result"); // Enum or Nullable<Enum>.

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

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public bool Equals(DefaultMemberMapping<TSource, TTarget> other)
        {
            return other != null
                && SourceMember.Equals(other.SourceMember)
                && TargetMember.Equals(other.TargetMember)
                && NullableBehaviour == other.NullableBehaviour;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as DefaultMemberMapping<TSource, TTarget>);
        }

        /// <summary>
        /// Returns the hash code of this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return Hash.Combine(
                SourceMember.GetHashCode(),
                TargetMember.GetHashCode(),
                NullableBehaviour.GetHashCode()
            );
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        public override string ToString()
        {
            return $"target.{TargetMember.Name} = source.{SourceMember.Name}";
        }
    }
}