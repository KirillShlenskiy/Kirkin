using System;
using System.Collections.Generic;
using System.Diagnostics;

using Kirkin.Collections.Generic;
using Kirkin.Reflection;

namespace Kirkin.ChangeTracking
{
    /// <summary>
    /// Tracks property values of a particular object
    /// and detects when a property value change occurs.
    /// </summary>
    public sealed class ChangeTracker<T>
        where T : class
    {
        internal PropertyValueSnapshot<T> Snapshot;

        /// <summary>
        /// The object being tracked.
        /// </summary>
        public T TrackedObject
        {
            get
            {
                return Snapshot.Target;
            }
        }

        /// <summary>
        /// Property values as at the time this instance was created.
        /// </summary>
        public Vector<PropertyValue> OriginalValues
        {
            // The reason to use a snapshot as opposed to a clone is that the
            // caller might be interested in the original values of read-only
            // properties, and there is no way to copy them onto the clone.
            get
            {
                return Snapshot.PropertyValues;
            }
        }

        /// <summary>
        /// Property definition collection definition that determines which of
        /// the object properties have their values tracked by this instance.
        /// </summary>
        public PropertyList<T> PropertyList
        {
            get
            {
                return Snapshot.PropertyList;
            }
        }

        /// <summary>
        /// Enumerates the current property values of the tracked object.
        /// </summary>
        public IEnumerable<PropertyValue> CurrentValues
        {
            get
            {
                foreach (IPropertyAccessor accessor in PropertyList.PropertyAccessors) {
                    yield return new PropertyValue(accessor.Property, accessor.GetValue(TrackedObject));
                }
            }
        }

        /// <summary>
        /// Creates a new change tracker which detects changes in all 
        /// properties of the given object which have a public getter.
        /// </summary>
        public ChangeTracker(T trackedObject)
            : this(trackedObject, PropertyList<T>.Default)
        {
        }

        /// <summary>
        /// Creates a new change tracker which detects changes in all 
        /// properties of the given object found in the given property list.
        /// </summary>
        public ChangeTracker(T trackedObject, PropertyList<T> propertyList)
        {
            if (trackedObject == null) throw new ArgumentNullException(nameof(trackedObject));
            if (propertyList == null) throw new ArgumentNullException(nameof(propertyList));

            Snapshot = new PropertyValueSnapshot<T>(trackedObject, propertyList);
        }

        /// <summary>
        /// Enumerates the differences between OriginalValues and CurrentValues.
        /// Only returns information on properties whose values are deemed to have changed.
        /// </summary>
        public IEnumerable<PropertyValueChange> DetectChanges()
        {
            Vector<PropertyValue> propertyValues = Snapshot.PropertyValues;

            Debug.Assert(propertyValues.Length == PropertyList.PropertyAccessors.Length);

            for (int i = 0; i < propertyValues.Length; i++)
            {
                PropertyValue original = propertyValues[i];
                IPropertyAccessor accessor = PropertyList.PropertyAccessors[i];

                //Debug.Assert(original.Property == accessor.Property, "Original and current value order mismatch.");
                if (original.Property != accessor.Property) {
                    throw new InvalidOperationException("Property order mismatch.");
                }

                object currentValue = accessor.GetValue(TrackedObject);

                // Comparison.
                if (!Equals(original.Value, currentValue)) {
                    yield return new PropertyValueChange(original.Property, original.Value, currentValue);
                }
            }
        }

        /// <summary>
        /// Resets the change tracker's <see cref="OriginalValues"/>
        /// to match the tracked object's current property values.
        /// </summary>
        public void Reset()
        {
            Snapshot = new PropertyValueSnapshot<T>(TrackedObject, PropertyList);
        }
    }
}