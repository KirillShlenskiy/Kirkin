using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using Kirkin.Linq.Expressions;

namespace Kirkin.Reflection
{
    /// <summary>
    /// Provides Reflection-related
    /// methods on the given type.
    /// </summary>
    internal static class PropertyAccessorFactory<T>
    {
        #region PropertyAccessor<T,> Cache

        /// <summary>
        /// IPropertyAccessor cache where the key is the name of the property and the
        /// value is a generic PropertyAccessor{T,} upcast to the non-generic interface.
        /// </summary>
        private static readonly ConcurrentDictionary<PropertyInfo, IPropertyAccessor> PropertyAccessors
            = new ConcurrentDictionary<PropertyInfo, IPropertyAccessor>(PropertyInfoEqualityComparer.Instance);

        #endregion

        #region Property overloads

        /// <summary>
        /// Provides fast access to the given public instance property.
        /// </summary>
        public static PropertyAccessor<T, TProperty> Property<TProperty>(Expression<Func<T, TProperty>> propertyExpr)
        {
            PropertyInfo propertyInfo = ExpressionUtil.Property(propertyExpr);

            // Resolve the cached entry or create a new one.
            IPropertyAccessor accessor;

            if (!PropertyAccessors.TryGetValue(propertyInfo, out accessor))
            {
                accessor = PropertyAccessors.GetOrAdd(
                    propertyInfo, new PropertyAccessor<T, TProperty>(propertyInfo)
                );
            }

            return (PropertyAccessor<T, TProperty>)accessor;
        }

        /// <summary>
        /// Provides fast access to the given public or non-public instance property.
        /// </summary>
        public static PropertyAccessor<T, TProperty>Property<TProperty>(string propertyName)
        {
            return Property<TProperty>(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Provides fast access to the given property.
        /// </summary>
        public static PropertyAccessor<T, TProperty> Property<TProperty>(string propertyName, BindingFlags bindingFlags)
        {
            PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName, bindingFlags);

            if (propertyInfo == null) {
                return null;
            }

            // Resolve the cached entry or create a new one.
            IPropertyAccessor accessor;

            if (!PropertyAccessors.TryGetValue(propertyInfo, out accessor))
            {
                accessor = PropertyAccessors.GetOrAdd(
                    propertyInfo, new PropertyAccessor<T, TProperty>(propertyInfo)
                );
            }

            return (PropertyAccessor<T, TProperty>)accessor;
        }

        /// <summary>
        /// Provides fast access to the given public or non-public instance property.
        /// </summary>
        public static IPropertyAccessor Property(string propertyName)
        {
            return Property(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Provides fast access to the given property.
        /// </summary>
        public static IPropertyAccessor Property(string propertyName, BindingFlags bindingFlags)
        {
            PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName, bindingFlags);

            return (propertyInfo == null) ? null : Property(propertyInfo);
        }

        /// <summary>
        /// Provides fast access to the given public or non-public instance property.
        /// </summary>
        public static IPropertyAccessor Property(PropertyInfo propertyInfo)
        {
            // Argument validation.
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

            // Resolve the cached entry or create a new one.
            IPropertyAccessor accessor;

            if (PropertyAccessors.TryGetValue(propertyInfo, out accessor)) {
                return accessor;
            }

            // PropertyInfo validation.
            // It is permissible for this type to store properties declared in T's base.
            if (!propertyInfo.DeclaringType.IsAssignableFrom(typeof(T))) {
                throw new InvalidOperationException("Property declaring type mismatch.");
            }

            // We'll use some Reflection to create a
            // generic PropertyAccessor, because it's faster
            // and ultimately that's what we want to cache.
            accessor = (IPropertyAccessor)Activator.CreateInstance(
                typeof(PropertyAccessor<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance,
                null,
                new[] { propertyInfo },
                null
            );

            return PropertyAccessors.GetOrAdd(propertyInfo, accessor);
        }

        #endregion
    }
}