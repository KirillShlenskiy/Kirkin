using System;

using Kirkin.Collections.Generic;

namespace Kirkin.ChangeTracking
{
    /// <summary>
    /// Snapshot of an object's property values at a particular point in time.
    /// </summary>
    internal sealed class PropertyValueSnapshot<T>
    {
        // Important: order of PropertyValues has to match the
        // order of property accessors defined by the type mapping.
        private readonly PropertyValue[] __propertyValues;

        /// <summary>
        /// Object whose property values were captured when this snapshot instance was created.
        /// </summary>
        public T Target { get; }

        /// <summary>
        /// Type mapping used to create this snapshot.
        /// </summary>
        public TypeMapping<T> TypeMapping { get; }

        /// <summary>
        /// Property values as at the time this instance was created.
        /// </summary>
        public Vector<PropertyValue> PropertyValues
        {
            get
            {
                return new Vector<PropertyValue>(__propertyValues);
            }
        }

        /// <summary>
        /// Creates a new snapshot of an object's property values at a particular point in time.
        /// </summary>
        internal PropertyValueSnapshot(T target, TypeMapping<T> typeMapping)
        {
            Target = target;
            TypeMapping = typeMapping;
            CapturePropertyValues(ref __propertyValues, target, typeMapping);
        }

        /// <summary>
        /// Captures property values at the current point in time.
        /// </summary>
        private static void CapturePropertyValues(ref PropertyValue[] propertyValues, T target, TypeMapping<T> typeMapping)
        {
            // The order or property values must match the order of property accessors defined 
            // by the type mapping. Otherwise ChangeTracker<T>.DetectChanges and other places will break.
            propertyValues = new PropertyValue[typeMapping.PropertyAccessors.Length];

            for (int i = 0; i < propertyValues.Length; i++)
            {
                TypeMapping<T>.PropertyAccessor accessor = typeMapping.PropertyAccessors[i];

                propertyValues[i] = new PropertyValue(accessor.Property, accessor.GetValue(target));
            }
        }

        /// <summary>
        /// Copies the property values captured by this instance to the given target.
        /// </summary>
        internal void Apply<TTarget>(TTarget target)
            where TTarget : T // Allow target to be derived from T.
        {
            for (int i = 0; i < TypeMapping.PropertyAccessors.Length; i++)
            {
                TypeMapping<T>.PropertyAccessor accessor = TypeMapping.PropertyAccessors[i];
                PropertyValue snapshotPropertyValue = __propertyValues[i];

                //Debug.Assert(accessor.Property == snapshotPropertyValue.Property, "Property order mismatch.");
                if (accessor.Property != snapshotPropertyValue.Property) {
                    throw new InvalidOperationException("Property order mismatch.");
                }

                if (accessor.Property.CanWrite)
                {
                    object newValue = snapshotPropertyValue.Value;
                    object oldValue = accessor.GetValue(target);

                    if (!Equals(newValue, oldValue)) {
                        accessor.SetValue(target, newValue);
                    }
                }
            }
        }
    }
}