using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Kirkin.Linq.Expressions;

namespace Kirkin.Reflection
{
    /// <summary>
    /// <see cref="IPropertyAccessor"/> factory methods.
    /// </summary>
    public static class PropertyAccessor
    {
        // Cached PropertyAccessFactory<>.Property(PropertyInfo) delegates.
        private static readonly ConcurrentDictionary<Type, Func<PropertyInfo, IPropertyAccessor>> GenericPropertyAccessorFactoryDelegates
            = new ConcurrentDictionary<Type, Func<PropertyInfo, IPropertyAccessor>>();

        #region Resolve overloads

        /// <summary>
        /// Returns an accessor for the property identified by the given expression.
        /// </summary>
        public static IPropertyAccessor Resolve<T>(Expression<Func<T, object>> propertyExpr)
        {
            if (propertyExpr == null) throw new ArgumentNullException(nameof(propertyExpr));

            PropertyInfo propertyInfo = ExpressionUtil.Property(propertyExpr);

            return Resolve(propertyInfo);
        }

        /// <summary>
        /// Provides fast access to the given public or non-public property.
        /// </summary>
        internal static IPropertyAccessor Resolve<T>(string propertyName,
                                                     BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            return Resolve(typeof(T), propertyName, bindingFlags);
        }

        /// <summary>
        /// Provides fast access to the given property.
        /// </summary>
        internal static IPropertyAccessor Resolve(Type type,
                                                  string propertyName,
                                                  BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            PropertyInfo propertyInfo = type.GetProperty(propertyName, bindingFlags);

            return propertyInfo == null ? null : Resolve(propertyInfo);
        }

        /// <summary>
        /// Provides fast access to the given public or non-public property.
        /// </summary>
        public static IPropertyAccessor Resolve(PropertyInfo propertyInfo)
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

        #region ResolveAll overloads

        /// <summary>
        /// Provides fast access to public instance properties.
        /// </summary>
        public static IEnumerable<IPropertyAccessor> ResolveAll<T>(BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            return ResolveAll(typeof(T), bindingFlags);
        }

        /// <summary>
        /// Provides fast access to properties matching the given binding flags.
        /// </summary>
        public static IEnumerable<IPropertyAccessor> ResolveAll(Type type,
                                                                BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (bindingFlags.HasFlag(BindingFlags.Static)) {
                throw new ArgumentException("BindingFlags.Static is not allowed.");
            }

            // Resolve fast properties.
            foreach (PropertyInfo propertyInfo in type.GetProperties(bindingFlags)) {
                yield return Resolve(propertyInfo);
            }
        }

        #endregion
    }
}