using System.Collections.Generic;
using System.Reflection;

using Kirkin.Utilities;

namespace Kirkin.Reflection
{
    /// <summary>
    /// <see cref="IEqualityComparer{T}"/> implementation which reliably compares
    /// <see cref="MemberInfo"/> objects. Required in cases where the <see cref="MemberInfo"/> objects
    /// are retrieved from different levels of type hierarchy (i.e. one from base and another from derived).
    /// </summary>
    internal sealed class MemberInfoEqualityComparer
        : IEqualityComparer<MemberInfo>
    {
        public static readonly MemberInfoEqualityComparer Instance = new MemberInfoEqualityComparer();

        private MemberInfoEqualityComparer()
        {
        }

        public bool Equals(MemberInfo x, MemberInfo y)
        {
            return ReferenceEquals(x, y) || (x.Module == y.Module && x.MetadataToken == y.MetadataToken);
        }

        public int GetHashCode(MemberInfo obj)
        {
            return Hash.Combine(obj.Module.GetHashCode(), obj.MetadataToken);
        }
    }
}