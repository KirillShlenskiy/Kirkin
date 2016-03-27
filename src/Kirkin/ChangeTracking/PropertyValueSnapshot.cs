using System;

using Kirkin.Collections.Generic;
using Kirkin.Reflection;

namespace Kirkin.ChangeTracking
{
    /// <summary>
    /// Snapshot of an object's property values at a particular point in time.
    /// </summary>
    internal sealed class PropertyValueSnapshot<T>
    {
        // Important: order of PropertyValues has to match the
        // order of property accessors defined by PropertyList.
        private readonly PropertyValue[] __propertyValues;

        /// <summary>
        /// Object whose property values were captured when this snapshot instance was created.
        /// </summary>
        public T Target { get; }

        /// <summary>
        /// List of properties used to create this snapshot.
        /// </summary>
        public PropertyList<T> PropertyList { get; }

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
        internal PropertyValueSnapshot(T target, PropertyList<T> propertyList)
        {
            Target = target;
            PropertyList = propertyList;
            __propertyValues = CapturePropertyValues(target, propertyList);
        }

        /// <summary>
        /// Captures property values at the current point in time.
        /// </summary>
        private static PropertyValue[] CapturePropertyValues(T target, PropertyList<T> propertyList)
        {
            // The order or property values must match the order of property accessors defined 
            // by the type mapping. Otherwise ChangeTracker<T>.DetectChanges and other places will break.
            PropertyValue[] propertyValues = new PropertyValue[propertyList.PropertyAccessors.Length];

            for (int i = 0; i < propertyValues.Length; i++)
            {
                IPropertyAccessor accessor = propertyList.PropertyAccessors[i];

                propertyValues[i] = new PropertyValue(accessor.Property, accessor.GetValue(target));
            }

            return propertyValues;
        }

        /// <summary>
        /// Copies the property values captured by this instance to the given target.
        /// </summary>
        internal void Apply<TTarget>(TTarget target)
            where TTarget : T // Allow target to be derived from T.
        {
            for (int i = 0; i < PropertyList.PropertyAccessors.Length; i++)
            {
                IPropertyAccessor accessor = PropertyList.PropertyAccessors[i];
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