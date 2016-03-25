﻿using System;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Kirkin.Reflection
{
    /// <summary>
    /// Provides Reflection-related and other util methods on <see cref="Type" />.
    /// </summary>
    public static class TypeUtil
    {
        #region Property overloads

        // Cached TypeUtil<>.Property(PropertyInfo) delegates.
        private static readonly ConcurrentDictionary<Type, Func<PropertyInfo, IPropertyAccessor>> GenericTypeUtilPropertyDelegates
            = new ConcurrentDictionary<Type, Func<PropertyInfo, IPropertyAccessor>>();

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
            if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");

            // Resolve cached TypeUtil<T>.Property(PropertyInfo)
            // delegate, or create a new one with Reflection.
            Func<PropertyInfo, IPropertyAccessor> propertyFunc;
            
            if (!GenericTypeUtilPropertyDelegates.TryGetValue(propertyInfo.DeclaringType, out propertyFunc))
            {
                Type typeUtilType = typeof(TypeUtil<>).MakeGenericType(propertyInfo.DeclaringType);
                MethodInfo propertyMethod = typeUtilType.GetMethod("Property", new[] { typeof(PropertyInfo) });

                var newPropertyFunc = (Func<PropertyInfo, IPropertyAccessor>)Delegate.CreateDelegate(
                    typeof(Func<PropertyInfo, IPropertyAccessor>), propertyMethod
                );

                propertyFunc = GenericTypeUtilPropertyDelegates.GetOrAdd(
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
            if (type == null) throw new ArgumentNullException("type");

            if (bindingFlags.HasFlag(BindingFlags.Static)) {
                throw new ArgumentException("BindingFlags.Static is not allowed.");
            }

            // Resolve fast properties.
            foreach (PropertyInfo propertyInfo in type.GetProperties(bindingFlags)) {
                yield return Property(propertyInfo);
            }
        }

        #endregion

        #region Method delegate resolution

        // Untested.

        ///// <summary>
        ///// Provides fast access to the given
        ///// public or non-public static method.
        ///// </summary>
        //public static TDelegate StaticMethod<TDelegate>(Type type, string methodName)
        //{
        //    if (type == null) throw new ArgumentNullException("type");

        //    MethodInfo method = type.GetMethod(
        //        methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
        //    );

        //    if (method == null)
        //    {
        //        throw new MissingMethodException(
        //            string.Format("{0}.{1} method cannot be resolved.", type.Name, methodName)
        //        );
        //    }

        //    // Ugly double cast but whatever.
        //    return (TDelegate)(object)Delegate.CreateDelegate(typeof(TDelegate), method);
        //}

        #endregion
    }
}