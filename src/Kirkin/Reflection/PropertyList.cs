using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Kirkin.Collections.Generic;
using Kirkin.Linq.Expressions;
using Kirkin.Utilities;

namespace Kirkin.Reflection
{
    /// <summary>
    /// Immutable collection of property definitions.
    /// </summary>
    public sealed class PropertyList<T>
    {
        /// <summary>
        /// Default property list fot type T. Encapsulates all readable public instance properties.
        /// </summary>
        public static PropertyList<T> Default { get; } = CreateDefault();

        /// <summary>
        /// Creates an instance of <see cref="PropertyList{T}"/> with default parameters.
        /// </summary>
        private static PropertyList<T> CreateDefault()
        {
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            ArrayBuilder<IPropertyAccessor> accessors = new ArrayBuilder<IPropertyAccessor>(properties.Length);

            foreach (PropertyInfo prop in properties)
            {
                if (prop.CanRead) {
                    accessors.UnsafeAdd(PropertyAccessorFactory.Resolve(prop));
                }
            }

            return new PropertyList<T>(accessors.ToArray());
        }

        /// <summary>
        /// Empty list of properties of type T.
        /// </summary>
        public static PropertyList<T> Empty { get; } = new PropertyList<T>(Array<IPropertyAccessor>.Empty);

        /// <summary>
        /// Backing field for <see cref="PropertyAccessors"/>.
        /// PERF: slightly faster than <see cref="Vector{IPropertyAccessor}"/>.
        /// </summary>
        private readonly IPropertyAccessor[] _propertyAccessors;

        /// <summary>
        /// Accessors for properties mapped by this instance.
        /// </summary>
        public Vector<IPropertyAccessor> PropertyAccessors
        {
            get
            {
                return new Vector<IPropertyAccessor>(_propertyAccessors);
            }
        }

        /// <summary>
        /// Properties mapped by this instance.
        /// </summary>
        internal PropertyInfo[] Properties
        {
            get
            {
                PropertyInfo[] properties = new PropertyInfo[_propertyAccessors.Length];

                for (int i = 0; i < _propertyAccessors.Length; i++) {
                    properties[i] = _propertyAccessors[i].Property;
                }

                return properties;
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
        /// Returns a new <see cref="PropertyList{T}"/> instance including the given property.
        /// </summary>
        public PropertyList<T> Including<TProperty>(Expression<Func<T, TProperty>> propertyExpr)
        {
            if (propertyExpr == null) throw new ArgumentNullException(nameof(propertyExpr));

            PropertyInfo includedProperty = ExpressionUtil.Property(propertyExpr);

            foreach (IPropertyAccessor accessor in _propertyAccessors)
            {
                if (MemberInfoEqualityComparer.Instance.Equals(accessor.Property, includedProperty)) {
                    return this;
                }
            }

            IPropertyAccessor[] accessors = new IPropertyAccessor[_propertyAccessors.Length + 1];

            if (_propertyAccessors.Length != 0) {
                Array.Copy(_propertyAccessors, 0, accessors, 0, _propertyAccessors.Length);
            }

            accessors[accessors.Length - 1] = PropertyAccessorFactory.Resolve(includedProperty);

            return new PropertyList<T>(accessors);
        }

        /// <summary>
        /// Returns a new instance of PropertyList
        /// with the given property excluded from
        /// the collection of mapped properties.
        /// </summary>
        public PropertyList<T> Without<TProperty>(Expression<Func<T, TProperty>> propertyExpr)
        {
            if (propertyExpr == null) throw new ArgumentNullException(nameof(propertyExpr));

            PropertyInfo excludedProperty = ExpressionUtil.Property(propertyExpr);
            ArrayBuilder<IPropertyAccessor> accessors = new ArrayBuilder<IPropertyAccessor>(_propertyAccessors.Length - 1);

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