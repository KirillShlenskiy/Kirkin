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
                return ExpressionTypeConverter.ChangeType(source, TargetMember.Type, NullableBehaviour);
            }

            return source;
        }

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