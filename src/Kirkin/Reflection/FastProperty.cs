#if !__MOBILE__

using System;
using System.Reflection;
using System.Reflection.Emit;

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
    ///   100% slower than direct property access, and
    ///   5+ times faster than traditional reflection.
    /// </remarks>
    public class FastProperty
    {
        // Read-only fields.
        private readonly bool ValueTypeNullSemantics;

        // Backing fields.
        private Func<object, object> _getter;
        private Action<object, object> _setter;

        /// <summary>
        /// Property specified when this instance was created.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Creates a new instance wrapping the given PropertyInfo.
        /// </summary>
        internal FastProperty(PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException("property");
            if (property.IsStatic()) throw new ArgumentException("The property cannot be static.");

            Property = property;
            ValueTypeNullSemantics = property.PropertyType.IsValueType;
        }

        /// <summary>
        /// Invokes the property getter.
        /// </summary>
        public object GetValue(object instance)
        {
            // The Getter property doesn't seem to get
            // inlined properly, so duplicating the code
            // here yields a slight performance improvement.
            //
            // This code obviously has a race condition,
            // but as long as the value of _getter is not
            // publically available, it doesn't really matter
            // if it happens to be initialised multiple times.
            if (_getter == null)
            {
                if (!Property.CanRead)
                {
                    throw new InvalidOperationException("The property does not define a getter.");
                }

                _getter = DynamicCreateGetter();
            }

            return _getter.Invoke(instance);
        }

        /// <summary>
        /// Invokes the property setter.
        /// </summary>
        public void SetValue(object instance, object value)
        {
            // The Setter property doesn't seem to get
            // inlined properly, so duplicating the code
            // here yields a slight performance improvement.
            //
            // This code obviously has a race condition,
            // but as long as the value of _setter is not
            // publically available, it doesn't really matter
            // if it happens to be initialised multiple times.
            if (_setter == null)
            {
                if (!Property.CanWrite)
                {
                    throw new InvalidOperationException("The property does not define a setter.");
                }

                _setter = DynamicCreateSetter();
            }

            if (ValueTypeNullSemantics && value == null)
            {
                // Value type handling consistent with PropertyInfo.SetValue(obj, null).
                _setter.Invoke(instance, Activator.CreateInstance(Property.PropertyType));
            }
            else
            {
                _setter.Invoke(instance, value);
            }
        }

        /// <summary>
        /// Creates a non-generic getter delegate.
        /// </summary>
        private Func<object, object> DynamicCreateGetter()
        {
            var getMethod = Property.GetGetMethod();

            if (getMethod == null)
            {
                throw new InvalidOperationException("Unable to resolve Get method.");
            }

            var getter = new DynamicMethod(
                name: "<FastProperty>_Get" + Property.Name,
                returnType: typeof(object),
                parameterTypes: new[] { typeof(object) },
                m: Property.Module,
                skipVisibility: true
            );

            var generator = getter.GetILGenerator();

            generator.DeclareLocal(typeof(object));
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Castclass, Property.DeclaringType);
            generator.EmitCall(OpCodes.Callvirt, getMethod, null);

            if (Property.PropertyType.IsValueType)
            {
                generator.Emit(OpCodes.Box, Property.PropertyType);
            }

            generator.Emit(OpCodes.Ret);

            return (Func<object, object>)getter.CreateDelegate(typeof(Func<object, object>));
        }

        /// <summary>
        /// Creates a non-generic setter delegate.
        /// </summary>
        private Action<object, object> DynamicCreateSetter()
        {
            var setMethod = Property.GetSetMethod();

            if (setMethod == null)
            {
                throw new InvalidOperationException("Unable to resolve Set method.");
            }

            var setter = new DynamicMethod(
                name: "<FastProperty>_Set" + Property.Name,
                returnType: typeof(void),
                parameterTypes: new[] { typeof(object), typeof(object) },
                m: Property.Module,
                skipVisibility: true
            );

            var generator = setter.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Castclass, Property.DeclaringType);
            generator.Emit(OpCodes.Ldarg_1);

            if (Property.PropertyType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox_Any, Property.PropertyType);
            }
            else
            {
                generator.Emit(OpCodes.Castclass, Property.PropertyType);
            }

            generator.EmitCall(OpCodes.Callvirt, setMethod, null);
            generator.Emit(OpCodes.Ret);

            return (Action<object, object>)setter.CreateDelegate(typeof(Action<object, object>));
        }
    }
}

#endif