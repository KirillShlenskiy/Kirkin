using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Kirkin.Collections.Generic;
using Kirkin.Linq.Expressions;
using Kirkin.Reflection;
using Kirkin.Utilities;

namespace Kirkin
{
    /// <summary>
    /// Reflection-based property mapper.
    /// </summary>
    public sealed class TypeMapping<T>
    {
        /// <summary>
        /// Default mapping instance for type T. Maps all properties
        /// which have an accessible getter for read (compare/hash)
        /// operations, and all properties which have accessible
        /// getters and setters for write (copy/clone) operations.
        /// </summary>
        public static TypeMapping<T> Default { get; } = new TypeMapping<T>(PropertyAccessor.ResolveAll<T>());

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
        private TypeMapping(IPropertyAccessor[] propertyAccessors)
        {
            _propertyAccessors = propertyAccessors;
        }

        /// <summary>
        /// Returns a new instance of TypeMapping
        /// with the given property excluded from
        /// the collection of mapped properties.
        /// </summary>
        public TypeMapping<T> Without<TProperty>(Expression<Func<T, TProperty>> propertyExpr)
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

            return new TypeMapping<T>(accessors.ToArray());
        }

        /// <summary>
        /// Creates a shallow clone of the given object.
        /// </summary>
        public TTarget Clone<TTarget>(TTarget original)
            where TTarget : T, new()
        {
            if (original == null) throw new ArgumentNullException(nameof(original));

            return Map(original, new TTarget());
        }

        /// <summary>
        /// Reconciles the differences where necessary,
        /// and returns the target instance.
        /// </summary>
        public TTarget Map<TTarget>(T source, TTarget target)
            where TTarget : T // Target can be derived from source.
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));

            foreach (IPropertyAccessor accessor in _propertyAccessors)
            {
                if (accessor.Property.CanWrite)
                {
                    object newValue = accessor.GetValue(source);
                    object oldValue = accessor.GetValue(target);

                    if (!Equals(newValue, oldValue)) {
                        accessor.SetValue(target, newValue);
                    }
                }
            }

            return target;
        }

        /// <summary>
        /// Reconciles the differences where necessary,
        /// and returns the target instance and number
        /// of changes applied.
        /// </summary>
        public TTarget Map<TTarget>(T source, TTarget target, out int changeCount)
            where TTarget : T // Target can be derived from source.
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));

            changeCount = 0;

            foreach (IPropertyAccessor accessor in _propertyAccessors)
            {
                if (accessor.Property.CanWrite)
                {
                    object newValue = accessor.GetValue(source);
                    object oldValue = accessor.GetValue(target);

                    if (!Equals(newValue, oldValue))
                    {
                        accessor.SetValue(target, newValue);
                        changeCount++;
                    }
                }
            }

            return target;
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