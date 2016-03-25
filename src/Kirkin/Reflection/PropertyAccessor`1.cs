using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using Kirkin.Linq.Expressions;

namespace Kirkin.Reflection
{
    /// <summary>
    /// <see cref="PropertyAccessor{TTarget, TProperty}"/> factory methods.
    /// </summary>
    public static class PropertyAccessor<T>
    {
        #region PropertyAccessor<T,> Cache

        /// <summary>
        /// IPropertyAccessor cache where the key is the name of the property and the
        /// value is a generic PropertyAccessor{T,} upcast to the non-generic interface.
        /// </summary>
        private static readonly ConcurrentDictionary<PropertyInfo, IPropertyAccessor> PropertyAccessors
            = new ConcurrentDictionary<PropertyInfo, IPropertyAccessor>(MemberInfoEqualityComparer.Instance);

        #endregion

        #region Property overloads

        /// <summary>
        /// Returns an accessor for the property identified by the given expression.
        /// </summary>
        public static PropertyAccessor<T, TProperty> Resolve<TProperty>(Expression<Func<T, TProperty>> propertyExpr)
        {
            PropertyInfo propertyInfo = ExpressionUtil.Property(propertyExpr);

            return ResolveFast<TProperty>(propertyInfo);
        }

        /// <summary>
        /// Returns an accessor for the property with the given name.
        /// </summary>
        public static PropertyAccessor<T, TProperty> Resolve<TProperty>(string propertyName,
                                                                        BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName, bindingFlags);

            return ResolveFast<TProperty>(propertyInfo);
        }

        /// <summary>
        /// Gets or creates an accessor for the given property.
        /// Faster than <see cref="GetOrCreateAccessor(PropertyInfo)"/>.
        /// </summary>
        private static PropertyAccessor<T, TProperty> ResolveFast<TProperty>(PropertyInfo propertyInfo)
        {
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
        internal static IPropertyAccessor GetOrCreateAccessor(PropertyInfo propertyInfo)
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