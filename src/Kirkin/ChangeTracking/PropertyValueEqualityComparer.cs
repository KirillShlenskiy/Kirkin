using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Kirkin.Reflection;

namespace Kirkin.ChangeTracking
{
    /// <summary>
    /// Arbitrary type equality comparer based on PropertyList.
    /// </summary>
    public sealed class PropertyValueEqualityComparer<T>
        : IEqualityComparer<T>
    {
        private static PropertyValueEqualityComparer<T> _default;

        /// <summary>
        /// <see cref="PropertyValueEqualityComparer{T}"/> which compares
        /// all readable public instance properties of <see cref="T"/>.
        /// </summary>
        public static PropertyValueEqualityComparer<T> Default
        {
            get
            {
                if (_default == null) {
                    _default = new PropertyValueEqualityComparer<T>(PropertyList<T>.Default);
                }

                return _default;
            }
        }

        /// <summary>
        /// Properties targeted by this comparer.
        /// </summary>
        public PropertyList<T> PropertyList { get; }

        /// <summary>
        /// Equality comparer used to compare property values for equality.
        /// </summary>
        public IReadOnlyDictionary<Type, IEqualityComparer> EqualityComparers { get; }

        /// <summary>
        /// Creates a new comparer instance based on the given <see cref="PropertyList{T}"/>.
        /// </summary>
        public PropertyValueEqualityComparer(PropertyList<T> propertyList)
        {
            if (propertyList == null) throw new ArgumentNullException(nameof(propertyList));

            PropertyList = propertyList;
        }

        /// <summary>
        /// Creates a new comparer instance based on the given <see cref="PropertyList{T}"/>.
        /// </summary>
        public PropertyValueEqualityComparer(PropertyList<T> propertyList, IReadOnlyDictionary<Type, IEqualityComparer> equalityComparers)
        {
            if (propertyList == null) throw new ArgumentNullException(nameof(propertyList));
            if (equalityComparers == null) throw new ArgumentNullException(nameof(equalityComparers));

            PropertyList = propertyList;
            EqualityComparers = equalityComparers;
        }

        /// <summary>
        /// Checks the given entities for equality
        /// based on their mapped property values.
        /// </summary>
        public bool Equals(T x, T y)
        {
            // Let's do a null check first.
            if (ReferenceEquals(x, null)) return ReferenceEquals(y, null);
            if (ReferenceEquals(y, null)) return false;

            foreach (IPropertyAccessor prop in PropertyList.PropertyAccessors)
            {
                IEqualityComparer comparer = ResolveComparer(prop.Property.PropertyType);
                object xValue = prop.GetValue(x);
                object yValue = prop.GetValue(y);

                if (!comparer.Equals(xValue, yValue))
                {
#if !PCL
                    Debug.Print("Inequality detected in {0}: {1} -> {2}.", prop.Property.Name, xValue, yValue);
#endif
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the product of hashcodes of
        /// the values of all mapped properties.
        /// </summary>
        public int GetHashCode(T obj)
        {
            unchecked // Overflow is fine, just wrap.
            {
                int hashCode = 17;

                foreach (IPropertyAccessor prop in PropertyList.PropertyAccessors)
                {
                    object value = prop.GetValue(obj);

                    hashCode = hashCode * 23 + ResolveComparer(prop.Property.PropertyType).GetHashCode(value);
                }

                return hashCode;
            }
        }

        // PERF: this needs to be substituted for 2 methods which
        // would fall through to object.Equals(object, object)
        // and object.GetHashCode respectively.
        private IEqualityComparer ResolveComparer(Type type)
        {
            IEqualityComparer comparer;

            return EqualityComparers != null && EqualityComparers.TryGetValue(type, out comparer)
                ? comparer
                : EqualityComparer<object>.Default;
        }
    }
}