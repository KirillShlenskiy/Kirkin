using System;
using System.Linq;

using Kirkin.Collections.Generic.Enumerators;
using Kirkin.Utilities;

namespace Kirkin.Mapping.Engine.Compilers
{
    /// <summary>
    /// Immutable list of mappings between source and target members.
    /// </summary>
    internal struct MemberMappingCollection<TSource, TTarget>
        : IEquatable<MemberMappingCollection<TSource, TTarget>>
    {
        private readonly MemberMapping<TSource, TTarget>[] Items;

        /// <summary>
        /// Combined <see cref="MemberMapping{TSource, TTarget}"/> hash.
        /// -1 means that the hash has been invalidated and needs to be recomputed.
        /// </summary>
        private readonly int PrecomputedHash;

        /// <summary>
        /// Creates a new instance of <see cref="MemberMappingCollection{TSource, TTarget}"/>.
        /// </summary>
        internal MemberMappingCollection(MemberMapping<TSource, TTarget>[] mappings)
        {
            Items = mappings;

            int hash = 0;

            foreach (MemberMapping<TSource, TTarget> mapping in mappings) {
                Hash.Combine(ref hash, mapping.GetHashCode());
            }

            PrecomputedHash = hash;
        }

        /// <summary>
        /// Returns a struct enumerator over the items in this collection.
        /// </summary>
        public ArrayEnumerator<MemberMapping<TSource, TTarget>> GetEnumerator()
        {
            return new ArrayEnumerator<MemberMapping<TSource, TTarget>>(Items);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public bool Equals(MemberMappingCollection<TSource, TTarget> other)
        {
            // PERF: No Items == null checks as this is an internal type and
            // we assume that instances will always initialized correctly.
            return ReferenceEquals(Items, other.Items)
                || Items.SequenceEqual(other.Items);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        public override bool Equals(object obj)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns this instance's precomputed hash.
        /// </summary>
        public override int GetHashCode()
        {
            return PrecomputedHash;
        }
    }
}