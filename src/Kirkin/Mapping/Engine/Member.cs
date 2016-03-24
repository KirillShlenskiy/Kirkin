using System;
using System.Linq.Expressions;

using Kirkin.Utilities;

namespace Kirkin.Mapping.Engine
{
    /// <summary>
    /// Mapped member definition.
    /// </summary>
    public abstract class Member : IEquatable<Member>
    {
        /// <summary>
        /// Name of the mapped member.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Runtime type of the member.
        /// </summary>
        public abstract Type Type { get; }

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
        public virtual bool Equals(Member other)
        {
            return other != null
                && other.GetType() == GetType() // Exact type in case ResolveGetter/Setter overridden.
                && other.Name == Name
                && other.Type == Type
                && other.CanRead == CanRead
                && other.CanWrite == CanWrite;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public sealed override bool Equals(object obj)
        {
            return Equals(obj as Member);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return Hash.Combine(
                GetType().GetHashCode(),
                Name.GetHashCode(),
                Type.GetHashCode()
            );
        }

        /// <summary>
        /// Returns a string describing the object
        /// which includes values of all public properties.
        /// </summary>
        public override string ToString()
        {
            return TypeMapping<Member>.Default.ToString(this);
        }
    }
}