#if !__MOBILE__

using System;
using System.Reflection;

namespace Kirkin.Reflection
{
    /// <summary>
    /// Provides fast access to property getter and setter.
    /// The instances of this type are meant to be cached
    /// and reused due to the considerable initial cost
    /// of creating getter and setter delegates incurred
    /// when Get or Set is called for the first time.
    /// </summary>
    /// <remarks>
    ///   On an x64 machine this is only roughly
    ///   20% slower than direct property access, and
    ///   10+ times faster than traditional reflection.
    /// </remarks>
    public sealed class FastProperty<TTarget, TProperty> : FastProperty
    {
        // Backing fields.
        private Func<TTarget, TProperty> _getter;
        private Action<TTarget, TProperty> _setter;

        /// <summary>
        /// Creates a new instance wrapping the given PropertyInfo.
        /// </summary>
        internal FastProperty(PropertyInfo property)
            : base(property)
        {
            // Argument null check is done in base.

            if (property.DeclaringType != typeof(TTarget))
                throw new ArgumentException("Property declaring type does not match fast property type.");

            if (property.PropertyType != typeof(TProperty))
                throw new ArgumentException("Property return type does not match fast property type.");
        }

        /// <summary>
        /// Invokes the property getter.
        /// </summary>
        public TProperty GetValue(TTarget instance)
        {
            // This code obviously has a race condition, but as long as _getter is not publicly
            // visible, it doesn't really matter if it happens to be initialised multiple times.
            if (_getter == null) {
                CreateGetter();
            }

            return _getter.Invoke(instance);
        }

        /// <summary>
        /// Invokes the property setter.
        /// </summary>
        public void SetValue(TTarget instance, TProperty value)
        {
            // This code obviously has a race condition, but as long as _setter is not publicly
            // visible, it doesn't really matter if it happens to be initialised multiple times.
            if (_setter == null) {
                CreateSetter();
            }

            _setter.Invoke(instance, value);
        }

        /// <summary>
        /// Creates and stores the getter delegate.
        /// </summary>
        private void CreateGetter()
        {
            if (!Property.CanRead) {
                throw new InvalidOperationException("The property does not define a getter.");
            }

            _getter = (Func<TTarget, TProperty>)Delegate.CreateDelegate(
                typeof(Func<TTarget, TProperty>), Property.GetGetMethod(nonPublic: true)
            );
        }

        /// <summary>
        /// Creates and stores the setter delegate.
        /// </summary>
        private void CreateSetter()
        {
            if (!Property.CanWrite) {
                throw new InvalidOperationException("The property does not define a setter.");
            }

            _setter = (Action<TTarget, TProperty>)Delegate.CreateDelegate(
                typeof(Action<TTarget, TProperty>), Property.GetSetMethod(nonPublic: true)
            );
        }
    }
}

#endif