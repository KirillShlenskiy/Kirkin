﻿using System;
using System.Linq.Expressions;

using Kirkin.Reflection;
using Kirkin.Utilities;

namespace Kirkin.Mapping
{
    /// <summary>
    /// Mapped member definition.
    /// </summary>
    public abstract class Member<T> : IEquatable<Member<T>>
    {
        /// <summary>
        /// Name of the mapped member.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Runtime type of the member.
        /// </summary>
        public abstract Type MemberType { get; }

        /// <summary>
        /// Returns true if this member supports read operations.
        /// </summary>
        public abstract bool CanRead { get; }

        /// <summary>
        /// Returns true if this member supports write operations.
        /// </summary>
        public abstract bool CanWrite { get; }

        /// <summary>
        /// Produces an expression which retrieves the relevant member value from the source.
        /// </summary>
        protected internal abstract Expression ResolveGetter(ParameterExpression source);

        /// <summary>
        /// Produces an expression which stores the value in the relevant member of the target.
        /// </summary>
        protected internal abstract Expression ResolveSetter(ParameterExpression target);

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public virtual bool Equals(Member<T> other)
        {
            return other != null
                && other.GetType() == GetType() // Exact type in case ResolveGetter/Setter overridden.
                && other.Name == Name
                && other.MemberType == MemberType
                && other.CanRead == CanRead
                && other.CanWrite == CanWrite;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public sealed override bool Equals(object obj)
        {
            return Equals(obj as Member<T>);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return Hash.Combine(
                GetType().GetHashCode(),
                Name.GetHashCode(),
                MemberType.GetHashCode()
            );
        }

        /// <summary>
        /// Returns a string describing the object
        /// which includes values of all public properties.
        /// </summary>
        public override string ToString()
        {
            return PropertyList<Member<T>>.Default.ToString(this);
        }
    }
}