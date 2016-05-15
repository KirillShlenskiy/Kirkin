using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using Kirkin.Linq.Expressions;

namespace Kirkin.Reflection
{
    /// <summary>
    /// <see cref="IPropertyAccessor"/> factory methods.
    /// </summary>
    public static class PropertyAccessorFactory
    {
        // Cached PropertyAccessor<>.GetOrCreateAccessor(PropertyInfo) delegates.
        private static readonly ConcurrentDictionary<Type, Func<PropertyInfo, IPropertyAccessor>> GetOrCreateAccessorDelegates
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
        /// Returns an accessor for the property with the given name.
        /// </summary>
        public static IPropertyAccessor Resolve<T>(string propertyName,
                                                   BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            return Resolve(typeof(T), propertyName, bindingFlags);
        }

        /// <summary>
        /// Returns an accessor for the property with the given name.
        /// </summary>
        public static IPropertyAccessor Resolve(Type type,
                                                string propertyName,
                                                BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            PropertyInfo propertyInfo = type.GetProperty(propertyName, bindingFlags);

            return propertyInfo == null ? null : Resolve(propertyInfo);
        }

        /// <summary>
        /// Returns an accessor for the given property.
        /// </summary>
        public static IPropertyAccessor Resolve(PropertyInfo propertyInfo)
        {
            // Argument validation.
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

            // Resolve cached PropertyAccessor<T>.GetOrCreateAccessor(PropertyInfo)
            // delegate, or create a new one using Reflection.
            Func<PropertyInfo, IPropertyAccessor> getOrCreateAccessorFunc;
            
            if (!GetOrCreateAccessorDelegates.TryGetValue(propertyInfo.DeclaringType, out getOrCreateAccessorFunc))
            {
                Type genericPropertyAccessorType = typeof(PropertyAccessorFactory<>).MakeGenericType(propertyInfo.DeclaringType);

                MethodInfo getOrCreateAccessorMethod = genericPropertyAccessorType.GetMethod(
                    nameof(PropertyAccessorFactory<object>.GetOrCreateAccessor),
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(PropertyInfo) },
                    null
                );

                var newPropertyFunc = (Func<PropertyInfo, IPropertyAccessor>)Delegate.CreateDelegate(
                    typeof(Func<PropertyInfo, IPropertyAccessor>), getOrCreateAccessorMethod
                );

                getOrCreateAccessorFunc = GetOrCreateAccessorDelegates.GetOrAdd(
                    propertyInfo.DeclaringType, newPropertyFunc
                );
            }

            return getOrCreateAccessorFunc(propertyInfo);
        }

        #endregion

        #region ResolveAll overloads

        /// <summary>
        /// Returns accessor for all properties matching the
        /// given binding flags (public, instance by default).
        /// </summary>
        /// <remarks>
        /// !!!!!!!!!!!!!!!!!!!!!!!! NOTE !!!!!!!!!!!!!!!!!!!!!!!!
        /// Don't make public unless you're certain that resolving
        /// all properties (including write-only ones) is the
        /// right thing to do. This can break Mapping.
        /// !!!!!!!!!!!!!!!!!!!!!!!! NOTE !!!!!!!!!!!!!!!!!!!!!!!!
        /// </remarks>
        internal static IPropertyAccessor[] ResolveAll<T>(BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            return ResolveAll(typeof(T), bindingFlags);
        }

        /// <summary>
        /// Returns accessor for all properties matching the
        /// given binding flags (public, instance by default).
        /// </summary>
        internal static IPropertyAccessor[] ResolveAll(Type type,
                                                       BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (bindingFlags.HasFlag(BindingFlags.Static)) {
                throw new ArgumentException("BindingFlags.Static is not allowed.");
            }

            PropertyInfo[] properties = type.GetProperties(bindingFlags);
            IPropertyAccessor[] accessors = new IPropertyAccessor[properties.Length];

            for (int i = 0; i < properties.Length; i++) {
                accessors[i] = Resolve(properties[i]);
            }

            return accessors;
        }

        #endregion
    }
}