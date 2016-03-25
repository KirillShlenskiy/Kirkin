﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using Kirkin.Linq.Expressions;

namespace Kirkin.Reflection
{
    /// <summary>
    /// Provides Reflection-related
    /// methods on the given type.
    /// </summary>
    public static class TypeUtil<T>
    {
        #region FastProperty<T,> Cache

        /// <summary>
        /// FastProperty cache where the key is the
        /// name of the property and the value is
        /// a generic FastProperty{T,} downcast to
        /// its non-generic FastProperty base.
        /// </summary>
        /// <remarks>
        ///   One of these is created per T ensuring
        ///   that the keys (property names) are unique.
        /// </remarks>
        private static readonly ConcurrentDictionary<PropertyInfo, IPropertyAccessor> FastProperties
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
            IPropertyAccessor fastProperty;

            if (!FastProperties.TryGetValue(propertyInfo, out fastProperty))
            {
                fastProperty = FastProperties.GetOrAdd(
                    propertyInfo, new PropertyAccessor<T, TProperty>(propertyInfo)
                );
            }

            return (PropertyAccessor<T, TProperty>)fastProperty;
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
            IPropertyAccessor fastProperty;

            if (!FastProperties.TryGetValue(propertyInfo, out fastProperty))
            {
                fastProperty = FastProperties.GetOrAdd(
                    propertyInfo, new PropertyAccessor<T, TProperty>(propertyInfo)
                );
            }

            return (PropertyAccessor<T, TProperty>)fastProperty;
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
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");

            // Resolve the cached entry or create a new one.
            IPropertyAccessor fastProperty;

            if (!FastProperties.TryGetValue(propertyInfo, out fastProperty))
            {
                // PropertyInfo validation.
                // It is permissible for TypeUtil<T> to
                // store properties declared in T's base.
                if (!propertyInfo.DeclaringType.IsAssignableFrom(typeof(T))) {
                    throw new InvalidOperationException("Property declaring type mismatch.");
                }

                // We'll use some Reflection to create a
                // generic FastProperty, because it's faster
                // and ultimately that's what we want to cache.
                Type closedType = typeof(PropertyAccessor<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
                ConstructorInfo constructor = closedType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(PropertyInfo) }, null);

                if (constructor == null) {
                    throw new MissingMethodException("Unable to resolve non-public " + closedType.Name + " constructor.");
                }

                fastProperty = FastProperties.GetOrAdd(
                    propertyInfo, (IPropertyAccessor)constructor.Invoke(new[] { propertyInfo })
                );
            }

            return fastProperty;
        }

        #endregion

        #region Properties overloads

        /// <summary>
        /// Provides fast access to public instance properties.
        /// </summary>
        public static IEnumerable<IPropertyAccessor> Properties()
        {
            return Properties(BindingFlags.Instance | BindingFlags.Public);
        }

        /// <summary>
        /// Provides fast access to instance properties matching the given binding flags.
        /// </summary>
        public static IEnumerable<IPropertyAccessor> Properties(BindingFlags bindingFlags)
        {
            if (bindingFlags.HasFlag(BindingFlags.Static)) {
                throw new ArgumentException("BindingFlags.Static is not allowed.");
            }

            // Resolve fast properties.
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties(bindingFlags)) {
                yield return Property(propertyInfo);
            }
        }

        #endregion

        #region Method delegate resolution

        /// <summary>
        /// Provides fast access to the given
        /// public or non-public static method.
        /// </summary>
        public static TDelegate StaticMethod<TDelegate>(string methodName)
        {
            Type type = typeof(T);

            MethodInfo method = type.GetMethod(
                methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (method == null)
            {
                throw new MissingMethodException(
                    string.Format("{0}.{1} method cannot be resolved.", type.Name, methodName)
                );
            }

            // Ugly double cast but whatever.
            return (TDelegate)(object)Delegate.CreateDelegate(typeof(TDelegate), method);
        }

        #endregion
    }
}