using System;
using System.Collections.Generic;
using System.Diagnostics;

using Kirkin.Reflection;

namespace Kirkin
{
    /// <summary>
    /// Arbitrary type equality comparer based on PropertyList.
    /// </summary>
    public class PropertyListEqualityComparer<T>
        : IEqualityComparer<T>
    {
        /// <summary>
        /// Properties targeted by this comparer.
        /// </summary>
        public PropertyList<T> PropertyList { get; }

        /// <summary>
        /// Creates a new comparer instance based on the default PropertyList.
        /// </summary>
        public PropertyListEqualityComparer()
            : this(PropertyList<T>.Default)
        {
        }

        /// <summary>
        /// Creates a new comparer instance based on the given PropertyList.
        /// </summary>
        public PropertyListEqualityComparer(PropertyList<T> propertyList)
        {
            if (propertyList == null) throw new ArgumentNullException(nameof(propertyList));

            PropertyList = propertyList;
        }

        /// <summary>
        /// Checks the given entities for equality
        /// based on their mapped property values.
        /// </summary>
        public virtual bool Equals(T x, T y)
        {
            // Let's do a null check first.
            if (ReferenceEquals(x, null)) return ReferenceEquals(y, null);
            if (ReferenceEquals(y, null)) return false;

            foreach (IPropertyAccessor prop in PropertyList.PropertyAccessors)
            {
                object xValue = prop.GetValue(x);
                object yValue = prop.GetValue(y);

                if (!Equals(xValue, yValue))
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
        /// Returns the sum of hashcodes of
        /// the values of all mapped properties.
        /// </summary>
        public virtual int GetHashCode(T obj)
        {
            unchecked // Overflow is fine, just wrap.
            {
                int hashCode = 17;

                foreach (IPropertyAccessor prop in PropertyList.PropertyAccessors)
                {
                    object value = prop.GetValue(obj);

                    hashCode = hashCode * 23 + (value == null ? 0 : value.GetHashCode());
                }

                return hashCode;
            }
        }

        /// <summary>
        /// Returns a new instance of PropertyListComparer
        /// with the given StringComparer option.
        /// </summary>
        public PropertyListEqualityComparer<T> WithStringComparer(StringComparer stringComparer)
        {
            if (stringComparer == null) throw new ArgumentNullException(nameof(stringComparer));

            return new PropertyListEqualityComparerWithStringComparer(PropertyList, stringComparer);
        }

        /// <summary>
        /// PropertyListComparer with StringComparer support for common scenarios.
        /// </summary>
        sealed class PropertyListEqualityComparerWithStringComparer
            : PropertyListEqualityComparer<T>
        {
            /// <summary>
            /// Comparer used to check string instances for equality.
            /// </summary>
            public StringComparer StringComparer { get; }

            public PropertyListEqualityComparerWithStringComparer(PropertyList<T> propertyList, StringComparer stringComparer)
                : base(propertyList)
            {
                if (stringComparer == null) throw new ArgumentNullException(nameof(stringComparer));

                StringComparer = stringComparer;
            }

            /// <summary>
            /// Checks the given entities for equality
            /// based on their mapped property values.
            /// </summary>
            public override bool Equals(T x, T y)
            {
                // Let's do a null check first.
                if (ReferenceEquals(x, null)) return ReferenceEquals(y, null);
                if (ReferenceEquals(y, null)) return false;

                foreach (IPropertyAccessor prop in PropertyList.PropertyAccessors)
                {
                    object xValue = prop.GetValue(x);
                    object yValue = prop.GetValue(y);

                    if (prop.Property.PropertyType == typeof(string))
                    {
                        string xString = (string)xValue;
                        string yString = (string)yValue;

                        if (xString == null) return yString == null;
                        if (yString == null) return false;

                        if (!StringComparer.Equals(xString, yString)) {
                            return false;
                        }
                    }
                    else if (!Equals(xValue, yValue))
                    {
                        return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// Returns the sum of hashcodes of
            /// the values of all mapped properties.
            /// </summary>
            public override int GetHashCode(T obj)
            {
                unchecked // Overflow is fine, just wrap.
                {
                    int hashCode = 17;

                    foreach (IPropertyAccessor prop in PropertyList.PropertyAccessors)
                    {
                        object value = prop.GetValue(obj);

                        if (prop.Property.PropertyType == typeof(string))
                        {
                            hashCode = hashCode * 23 + (value == null ? 0 : StringComparer.GetHashCode((string)value));
                        }
                        else
                        {
                            hashCode = hashCode * 23 + (value == null ? 0 : value.GetHashCode());
                        }
                    }

                    return hashCode;
                }
            }
        }
    }
}