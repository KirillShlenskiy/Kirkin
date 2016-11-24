﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Kirkin.Collections.Generic;
using Kirkin.Utilities;

namespace Kirkin.Mapping.Engine.MemberMappings
{
    /// <summary>
    /// Direct mapping between source and target members with support for conversions.
    /// </summary>
    internal sealed class DefaultMemberMapping<TSource, TTarget>
        : MemberMapping<TSource, TTarget>
        , IEquatable<DefaultMemberMapping<TSource, TTarget>>
    {
        private readonly IValueConversion[] _allowedConversions;

        /// <summary>
        /// Mapping source member.
        /// </summary>
        public Member<TSource> SourceMember { get; }
        
        /// <summary>
        /// Value conversions supported by this instance.
        /// </summary>
        public Vector<IValueConversion> AllowedConversions { get; }

        /// <summary>
        /// Nullable to non-nullable conversion behaviour of this instance.
        /// </summary>
        public NullableBehaviour NullableBehaviour { get; }

        /// <summary>
        /// Creates a new simple member-to-member mapping instance.
        /// </summary>
        public DefaultMemberMapping(Member<TSource> sourceMember, Member<TTarget> targetMember, IEnumerable<IValueConversion> allowedConversions, NullableBehaviour nullableBehaviour)
            : base(targetMember)
        {
            SourceMember = sourceMember;
            _allowedConversions = allowedConversions.ToArray();
            NullableBehaviour = nullableBehaviour;
        }

        /// <summary>
        /// Produces an expression which resolves the source value.
        /// </summary>
        internal override Expression GetSourceValueExpression(ParameterExpression sourceParam)
        {
            // Resolve and convert value automatically.
            Expression source = SourceMember.ResolveGetter(sourceParam);

            if (SourceMember.MemberType != TargetMember.MemberType)
            {
                Expression convertedSource;

                foreach (IValueConversion conversion in _allowedConversions)
                {
                    if (conversion.TryConvert(source, TargetMember.MemberType, NullableBehaviour, out convertedSource)) {
                        return convertedSource;
                    }
                }
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