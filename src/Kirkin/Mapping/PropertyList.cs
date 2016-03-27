using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Kirkin.Collections.Generic;
using Kirkin.Linq.Expressions;
using Kirkin.Reflection;
using Kirkin.Utilities;

namespace Kirkin.Mapping
{
    /// <summary>
    /// Immutable collection of property definitions.
    /// </summary>
    public sealed class PropertyList<T>
    {
        /// <summary>
        /// Default mapping instance for type T. Maps all properties
        /// which have an accessible getter for read (compare/hash)
        /// operations, and all properties which have accessible
        /// getters and setters for write (copy/clone) operations.
        /// </summary>
        public static PropertyList<T> Default { get; } = new PropertyList<T>(PropertyAccessor.ResolveAll<T>());

        // PERF: slightly faster than Vector<T>.
        private readonly IPropertyAccessor[] _propertyAccessors;

        /// <summary>
        /// Accessors for readable properties mapped by this instance.
        /// </summary>
        public Vector<IPropertyAccessor> PropertyAccessors
        {
            get
            {
                return new Vector<IPropertyAccessor>(_propertyAccessors);
            }
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        private PropertyList(IPropertyAccessor[] propertyAccessors)
        {
            _propertyAccessors = propertyAccessors;
        }

        /// <summary>
        /// Returns a new instance of TypeMapping
        /// with the given property excluded from
        /// the collection of mapped properties.
        /// </summary>
        public PropertyList<T> Without<TProperty>(Expression<Func<T, TProperty>> propertyExpr)
        {
            if (propertyExpr == null) throw new ArgumentNullException(nameof(propertyExpr));

            PropertyInfo excludedProperty = ExpressionUtil.Property(propertyExpr);
            Array<IPropertyAccessor>.Builder accessors = new Array<IPropertyAccessor>.Builder(_propertyAccessors.Length - 1);

            foreach (IPropertyAccessor accessor in _propertyAccessors)
            {
                if (!MemberInfoEqualityComparer.Instance.Equals(accessor.Property, excludedProperty)) {
                    accessors.Add(accessor);
                }
            }

            return new PropertyList<T>(accessors.ToArray());
        }

        /// <summary>
        /// Returns a string describing the object
        /// which includes values of all mapped properties.
        /// </summary>
        public string ToString(T obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            // Comma-separated list of
            // property names and their values.
            StringBuilder sb = new StringBuilder();
            Type actualType = obj.GetType();

            sb.Append(TypeName.NameIncludingGenericArguments(actualType));
            sb.Append(" { ");

            bool needComma = false;

            foreach (IPropertyAccessor accessor in _propertyAccessors)
            {
                if (needComma) {
                    sb.Append(", ");
                }

                sb.Append(accessor.Property.Name);
                sb.Append(" = ");
                sb.Append(accessor.GetValue(obj));

                needComma = true;
            }

            sb.Append(" }");

            return sb.ToString();
        }
    }
}