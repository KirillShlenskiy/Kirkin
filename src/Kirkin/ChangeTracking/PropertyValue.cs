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
        private readonly PropertyInfo __property;
        private readonly object __value;

        /// <summary>
        /// Property whose value this instance encapsulates.
        /// </summary>
        public PropertyInfo Property
        {
            get { return __property; }
        }

        /// <summary>
        /// Actual value of the property.
        /// </summary>
        public object Value
        {
            get { return __value; }
        }

        internal PropertyValue(PropertyInfo property, object value)
        {
            Debug.Assert(property != null, "Property cannot be null.");

            __property = property;
            __value = value;
        }
    }
}