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
            get
            {
                return Snapshot.PropertyValues;
            }
        }

        /// <summary>
        /// Property mapping definition that determines which of the
        /// object properties have their values tracked by this instance.
        /// </summary>
        public TypeMapping<T> TypeMapping
        {
            get
            {
                return Snapshot.TypeMapping;
            }
        }

        /// <summary>
        /// Enumerates the current property values of the tracked object.
        /// </summary>
        public IEnumerable<PropertyValue> CurrentValues
        {
            get
            {
                foreach (var p in TypeMapping.PropertyAccessors) {
                    yield return new PropertyValue(p.Property, p.GetValue(TrackedObject));
                }
            }
        }

        /// <summary>
        /// Creates a new change tracker which detects changes in all 
        /// properties of the given object which have a public getter.
        /// </summary>
        public ChangeTracker(T trackedObject)
            : this(trackedObject, TypeMapping<T>.Default)
        {
        }

        /// <summary>
        /// Creates a new change tracker which detects changes in all 
        /// properties of the given object specified by the given mapping.
        /// </summary>
        public ChangeTracker(T trackedObject, TypeMapping<T> typeMapping)
        {
            if (trackedObject == null) throw new ArgumentNullException("trackedObject");
            if (typeMapping == null) throw new ArgumentNullException("typeMapping");

            Snapshot = typeMapping.Snapshot(trackedObject);
        }

        /// <summary>
        /// Enumerates the differences between OriginalValues and CurrentValues.
        /// Only returns information on properties whose values are deemed to have changed.
        /// </summary>
        public IEnumerable<PropertyValueChange> DetectChanges()
        {
            Vector<PropertyValue> propertyValues = Snapshot.PropertyValues;

            Debug.Assert(propertyValues.Length == TypeMapping.PropertyAccessors.Length);

            for (int i = 0; i < propertyValues.Length; i++)
            {
                PropertyValue original = propertyValues[i];
                IPropertyAccessor accessor = TypeMapping.PropertyAccessors[i];

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
            Snapshot = TypeMapping.Snapshot(TrackedObject);
        }
    }
}