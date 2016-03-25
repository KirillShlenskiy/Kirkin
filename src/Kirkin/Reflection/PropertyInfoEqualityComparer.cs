using System.Collections.Generic;
using System.Reflection;

using Kirkin.Utilities;

namespace Kirkin.Reflection
{
    /// <summary>
    /// <see cref="IEqualityComparer{T}"/> implementation which reliably compares
    /// <see cref="PropertyInfo"/> objects. Required in cases where the <see cref="PropertyInfo"/> objects
    /// are retrieved from different levels of type hierarchy (i.e. one from base and another from derived).
    /// </summary>
    internal class PropertyInfoEqualityComparer
        : IEqualityComparer<PropertyInfo>
    {
        public static readonly PropertyInfoEqualityComparer Instance = new PropertyInfoEqualityComparer();

        private PropertyInfoEqualityComparer()
        {
        }

        public bool Equals(PropertyInfo x, PropertyInfo y)
        {
            if (ReferenceEquals(x, y)) {
                return true;
            }

            return x.Module == y.Module
                && x.MetadataToken == y.MetadataToken;
        }

        public int GetHashCode(PropertyInfo obj)
        {
            return Hash.Combine(obj.Module.GetHashCode(), obj.MetadataToken);
        }
    }
}