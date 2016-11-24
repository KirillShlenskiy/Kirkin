using System;
using System.Linq.Expressions;

using Kirkin.Utilities;

namespace Kirkin.Mapping.Engine.MemberMappings
{
    /// <summary>
    /// Direct mapping between source and target members with support for conversions.
    /// </summary>
    internal sealed class DirectMemberMapping<TSource, TTarget>
        : MemberMapping<TSource, TTarget>
        , IEquatable<DirectMemberMapping<TSource, TTarget>>
    {
        /// <summary>
        /// Mapping source member.
        /// </summary>
        public Member<TSource> SourceMember { get; }

        /// <summary>
        /// Creates a new simple member-to-member mapping instance.
        /// </summary>
        public DirectMemberMapping(Member<TSource> sourceMember, Member<TTarget> targetMember)
            : base(targetMember)
        {
            if (!targetMember.MemberType.IsAssignableFrom(sourceMember.MemberType))
            {
                throw new MappingException(
                    $"Value of member {typeof(TSource).Name}.{sourceMember.Name} ({sourceMember.MemberType.Name}) is not " +
                    $"directly assignable to {typeof(TTarget).Name}{targetMember.Name} ({targetMember.MemberType.Name})."
                );
            }

            SourceMember = sourceMember;
        }

        /// <summary>
        /// Produces an expression which resolves the source value.
        /// </summary>
        internal override Expression GetSourceValueExpression(ParameterExpression sourceParam)
        {
            return SourceMember.ResolveGetter(sourceParam);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public bool Equals(DirectMemberMapping<TSource, TTarget> other)
        {
            return other != null
                && SourceMember.Equals(other.SourceMember)
                && TargetMember.Equals(other.TargetMember)
                && GetType() == other.GetType();
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object obj)
        {
            return Equals(obj as DirectMemberMapping<TSource, TTarget>);
        }

        /// <summary>
        /// Returns the hash code of this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return Hash.Combine(
                SourceMember.GetHashCode(),
                TargetMember.GetHashCode(),
                GetType().GetHashCode()
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