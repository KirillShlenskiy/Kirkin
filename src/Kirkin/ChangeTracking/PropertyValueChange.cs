using System;
using System.Reflection;

namespace Kirkin.ChangeTracking
{
    /// <summary>
    /// Encapsulates a PropertyInfo reference and the
    /// original and current values of the associated property.
    /// </summary>
    public sealed class PropertyValueChange
    {
        /// <summary>
        /// Property whose value this instance encapsulates.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Original value of the property.
        /// </summary>
        public object OriginalValue { get; }

        /// <summary>
        /// Current value of the property.
        /// </summary>
        public object CurrentValue { get; }

        /// <summary>
        /// Initialises a new instance with the given PropertyInfo, original and current values.
        /// </summary>
        internal PropertyValueChange(PropertyInfo property, object originalValue, object currentValue)
        {
            if (property == null) throw new ArgumentNullException("property");

            Property = property;
            OriginalValue = originalValue;
            CurrentValue = currentValue;
        }

        /// <summary>
        /// Returns a string representation of the change.
        /// </summary>
        public override string ToString()
        {
            return $"{Property.Name}: {OriginalValue ?? "null"} -> {CurrentValue ?? "null"}";
        }
    }
}