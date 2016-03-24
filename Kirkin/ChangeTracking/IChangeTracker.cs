using System.Collections.Generic;

using Kirkin.Collections.Generic;

namespace Kirkin.ChangeTracking
{
    /// <summary>
    /// Contract for types which track property values of a particular
    /// object and detect when a property value change occurs.
    /// </summary>
    internal interface IChangeTracker<T> where T : class
    {
        /// <summary>
        /// The object being tracked.
        /// </summary>
        T TrackedObject { get; }

        /// <summary>
        /// Property values as at the time this instance was created.
        /// </summary>
        Vector<PropertyValue> OriginalValues { get; }

        /// <summary>
        /// Property mapping definition that determines which of the
        /// object properties have their values tracked by this instance.
        /// </summary>
        TypeMapping<T> TypeMapping { get; }

        /// <summary>
        /// Enumerates the current property values of the tracked object.
        /// </summary>
        IEnumerable<PropertyValue> CurrentValues { get; }

        /// <summary>
        /// Enumerates the differences between OriginalValues and CurrentValues.
        /// Only returns information on properties whose values are deemed to have changed.
        /// </summary>
        IEnumerable<PropertyValueChange> DetectChanges();
    }
}
