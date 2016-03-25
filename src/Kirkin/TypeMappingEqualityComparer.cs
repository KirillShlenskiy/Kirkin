using System;
using System.Collections.Generic;
using System.Diagnostics;

using Kirkin.Reflection;

namespace Kirkin
{
    /// <summary>
    /// Arbitrary type equality comparer based on TypeMapping.
    /// </summary>
    public class TypeMappingEqualityComparer<T>
        : IEqualityComparer<T>
    {
        /// <summary>
        /// Mapping which defines all properties used by this comparer.
        /// </summary>
        public TypeMapping<T> TypeMapping { get; }

        /// <summary>
        /// Creates a new comparer instance based on the default TypeMapping.
        /// </summary>
        public TypeMappingEqualityComparer()
            : this(TypeMapping<T>.Default)
        {
        }

        /// <summary>
        /// Creates a new comparer instance based on the given TypeMapping.
        /// </summary>
        public TypeMappingEqualityComparer(TypeMapping<T> typeMapping)
        {
            if (typeMapping == null) throw new ArgumentNullException("typeMapping");

            TypeMapping = typeMapping;
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

            foreach (IPropertyAccessor prop in TypeMapping.PropertyAccessors)
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

                foreach (IPropertyAccessor prop in TypeMapping.PropertyAccessors)
                {
                    object value = prop.GetValue(obj);

                    hashCode = hashCode * 23 + (value == null ? 0 : value.GetHashCode());
                }

                return hashCode;
            }
        }

        /// <summary>
        /// Returns a new instance of TypeMappingComparer
        /// with the given StringComparer option.
        /// </summary>
        public TypeMappingEqualityComparer<T> WithStringComparer(StringComparer stringComparer)
        {
            if (stringComparer == null) throw new ArgumentNullException("stringComparer");

            return new TypeMappingEqualityComparerWithStringComparer(TypeMapping, stringComparer);
        }

        /// <summary>
        /// TypeMappingComparer with StringComparer support for common scenarios.
        /// </summary>
        sealed class TypeMappingEqualityComparerWithStringComparer
            : TypeMappingEqualityComparer<T>
        {
            /// <summary>
            /// Comparer used by this mapping to check string instances for equality.
            /// </summary>
            public StringComparer StringComparer { get; }

            public TypeMappingEqualityComparerWithStringComparer(TypeMapping<T> typeMapping, StringComparer stringComparer)
                : base(typeMapping)
            {
                if (stringComparer == null) throw new ArgumentNullException("stringComparer");

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

                foreach (IPropertyAccessor prop in TypeMapping.PropertyAccessors)
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

                    foreach (IPropertyAccessor prop in TypeMapping.PropertyAccessors)
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