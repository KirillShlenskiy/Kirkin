using System.Diagnostics;
using System.Reflection;

namespace Kirkin.ChangeTracking
{
    /// <summary>
    /// Encapsulates a PropertyInfo reference and
    /// the value of the associated property.
    /// </summary>
    public struct PropertyValue
    {
        /// <summary>
        /// Property whose value this instance encapsulates.
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Actual value of the property.
        /// </summary>
        public object Value { get; }

        internal PropertyValue(PropertyInfo property, object value)
        {
            Debug.Assert(property != null, "Property cannot be null.");

            Property = property;
            Value = value;
        }
    }
}