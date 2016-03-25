﻿#if !__MOBILE__

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
    public sealed class FastProperty<TTarget, TProperty>
        : IFastProperty
    {
        /// <summary>
        /// Property specified when this instance was created.
        /// </summary>
        public PropertyInfo Property { get; }

        // Backing fields.
        private Func<TTarget, TProperty> _getter;
        private Action<TTarget, TProperty> _setter;

        /// <summary>
        /// Creates a new instance wrapping the given PropertyInfo.
        /// </summary>
        internal FastProperty(PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (property.IsStatic()) throw new ArgumentException("The property cannot be static.");
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
            if (_getter == null) {
                _getter = CreateGetter(Property);
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
                _setter = CreateSetter(Property);
            }

            _setter.Invoke(instance, value);
        }

        /// <summary>
        /// Creates and stores the getter delegate.
        /// </summary>
        private static Func<TTarget, TProperty> CreateGetter(PropertyInfo property)
        {
            if (!property.CanRead) {
                throw new InvalidOperationException("The property does not define a getter.");
            }

            return (Func<TTarget, TProperty>)Delegate.CreateDelegate(
                typeof(Func<TTarget, TProperty>), property.GetGetMethod(nonPublic: true)
            );
        }

        /// <summary>
        /// Creates and stores the setter delegate.
        /// </summary>
        private static Action<TTarget, TProperty> CreateSetter(PropertyInfo property)
        {
            if (!property.CanWrite) {
                throw new InvalidOperationException("The property does not define a setter.");
            }

            return (Action<TTarget, TProperty>)Delegate.CreateDelegate(
                typeof(Action<TTarget, TProperty>), property.GetSetMethod(nonPublic: true)
            );
        }

        object IFastProperty.GetValue(object instance)
        {
            return GetValue((TTarget)instance);
        }

        void IFastProperty.SetValue(object instance, object value)
        {
            SetValue((TTarget)instance, (TProperty)value);
        }
    }
}

#endif