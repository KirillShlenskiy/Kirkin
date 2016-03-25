using System;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Kirkin.Reflection
{
    /// <summary>
    /// Provides Reflection-related and other util methods on <see cref="Type" />.
    /// </summary>
    public static class PropertyAccessorFactory
    {
        // Cached PropertyAccessFactory<>.Property(PropertyInfo) delegates.
        private static readonly ConcurrentDictionary<Type, Func<PropertyInfo, IPropertyAccessor>> GenericPropertyAccessorFactoryDelegates
            = new ConcurrentDictionary<Type, Func<PropertyInfo, IPropertyAccessor>>();

        #region Property overloads

        /// <summary>
        /// Provides fast access to the given public or non-public property.
        /// </summary>
        public static IPropertyAccessor Property(Type type, string propertyName)
        {
            return Property(type, propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Provides fast access to the given property.
        /// </summary>
        public static IPropertyAccessor Property(Type type, string propertyName, BindingFlags bindingFlags)
        {
            PropertyInfo propertyInfo = type.GetProperty(propertyName, bindingFlags);

            return propertyInfo == null ? null : Property(propertyInfo);
        }

        /// <summary>
        /// Provides fast access to the given public or non-public property.
        /// </summary>
        public static IPropertyAccessor Property(PropertyInfo propertyInfo)
        {
            // Argument validation.
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

            // Resolve cached PropertyAccessFactory<T>.Property(PropertyInfo)
            // delegate, or create a new one with Reflection.
            Func<PropertyInfo, IPropertyAccessor> propertyFunc;
            
            if (!GenericPropertyAccessorFactoryDelegates.TryGetValue(propertyInfo.DeclaringType, out propertyFunc))
            {
                Type genericPropertyAccessorFactoryType = typeof(PropertyAccessorFactory<>).MakeGenericType(propertyInfo.DeclaringType);

                MethodInfo propertyMethod = genericPropertyAccessorFactoryType.GetMethod(
                    nameof(PropertyAccessorFactory<object>.Property), new[] { typeof(PropertyInfo) }
                );

                var newPropertyFunc = (Func<PropertyInfo, IPropertyAccessor>)Delegate.CreateDelegate(
                    typeof(Func<PropertyInfo, IPropertyAccessor>), propertyMethod
                );

                propertyFunc = GenericPropertyAccessorFactoryDelegates.GetOrAdd(
                    propertyInfo.DeclaringType, newPropertyFunc
                );
            }

            return propertyFunc(propertyInfo);
        }

        #endregion

        #region Properties overloads

        /// <summary>
        /// Provides fast access to public instance properties.
        /// </summary>
        public static IEnumerable<IPropertyAccessor> Properties(Type type)
        {
            return Properties(type, BindingFlags.Instance | BindingFlags.Public);
        }

        /// <summary>
        /// Provides fast access to properties matching the given binding flags.
        /// </summary>
        public static IEnumerable<IPropertyAccessor> Properties(Type type, BindingFlags bindingFlags)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (bindingFlags.HasFlag(BindingFlags.Static)) {
                throw new ArgumentException("BindingFlags.Static is not allowed.");
            }

            // Resolve fast properties.
            foreach (PropertyInfo propertyInfo in type.GetProperties(bindingFlags)) {
                yield return Property(propertyInfo);
            }
        }

        #endregion
    }
}