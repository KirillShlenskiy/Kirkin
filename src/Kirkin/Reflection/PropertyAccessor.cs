using System;
using System.Reflection;

namespace Kirkin.Reflection
{
    /// <summary>
    /// Provides fast access to property getter and setter. 
    /// </summary>
    /// <remarks>
    /// Instances of this type are meant to be cached and reused due to
    /// the considerable cost of compiling getter and setter delegates
    /// incurred when GetValue and SetValue are called for the first time.
    /// </remarks>
    public sealed class PropertyAccessor<TTarget, TProperty>
        : IPropertyAccessor
    {
        /// <summary>
        /// Property specified when this instance was created.
        /// </summary>
        public PropertyInfo Property { get; }

        // Backing fields.
        private Func<TTarget, TProperty> _compiledGetter;
        private Action<TTarget, TProperty> _compiledSetter;

        /// <summary>
        /// Creates a new instance wrapping the given PropertyInfo.
        /// </summary>
        internal PropertyAccessor(PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (IsStatic(property)) throw new ArgumentException("The property cannot be static.");
            if (property.DeclaringType != typeof(TTarget)) throw new ArgumentException("Property declaring type does not match fast property type.");
            if (property.PropertyType != typeof(TProperty)) throw new ArgumentException("Property return type does not match fast property type.");

            Property = property;
        }

        /// <summary>
        /// Invokes the property getter.
        /// </summary>
        public TProperty GetValue(TTarget instance)
        {
            // This code obviously has a race condition, but as long as _getter is not publicly
            // visible, it doesn't really matter if it happens to be initialised multiple times.
            if (_compiledGetter == null) {
                _compiledGetter = CompileGetter(Property);
            }

            return _compiledGetter(instance);
        }

        /// <summary>
        /// Invokes the property setter.
        /// </summary>
        public void SetValue(TTarget instance, TProperty value)
        {
            // This code obviously has a race condition, but as long as _setter is not publicly
            // visible, it doesn't really matter if it happens to be initialised multiple times.
            if (_compiledSetter == null) {
                _compiledSetter = CompileSetter(Property);
            }

            _compiledSetter(instance, value);
        }

        /// <summary>
        /// Explicit non-generic getter implementation.
        /// </summary>
        object IPropertyAccessor.GetValue(object instance)
        {
            return GetValue((TTarget)instance);
        }

        /// <summary>
        /// Explicit non-generic setter implementation.
        /// </summary>
        void IPropertyAccessor.SetValue(object instance, object value)
        {
            SetValue((TTarget)instance, (TProperty)value);
        }

        /// <summary>
        /// Creates and stores the getter delegate.
        /// </summary>
        private static Func<TTarget, TProperty> CompileGetter(PropertyInfo property)
        {
            if (!property.CanRead) {
                throw new InvalidOperationException("The property does not define a getter.");
            }
#if __MOBILE__
            // TODO: compiled delegate on platforms that support it?
            return target => (TProperty)property.GetValue(target);
#else
            return (Func<TTarget, TProperty>)Delegate.CreateDelegate(
                typeof(Func<TTarget, TProperty>), property.GetGetMethod(nonPublic: true)
            );
#endif
        }

        /// <summary>
        /// Creates and stores the setter delegate.
        /// </summary>
        private static Action<TTarget, TProperty> CompileSetter(PropertyInfo property)
        {
            if (!property.CanWrite) {
                throw new InvalidOperationException("The property does not define a setter.");
            }
#if __MOBILE__
            // TODO: compiled delegate on platforms that support it?
            return (target, value) => property.SetValue(target, value);
#else
            return (Action<TTarget, TProperty>)Delegate.CreateDelegate(
                typeof(Action<TTarget, TProperty>), property.GetSetMethod(nonPublic: true)
            );
#endif
        }

        /// <summary>
        /// Determines whether this PropertyInfo
        /// instance describes a static property.
        /// </summary>
        internal static bool IsStatic(PropertyInfo propertyInfo)
        {
            // Check the getter.
            if (propertyInfo.CanRead) {
                return propertyInfo.GetGetMethod(nonPublic: true).IsStatic;
            }

            // Check the setter.
            if (propertyInfo.CanWrite) {
                return propertyInfo.GetSetMethod(nonPublic: true).IsStatic;
            }

            // Ask the declaring type - slightly slower.
            PropertyInfo staticPropertyInfo = propertyInfo.DeclaringType.GetProperty(
                propertyInfo.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
            );

            return staticPropertyInfo != null;
        }
    }
}