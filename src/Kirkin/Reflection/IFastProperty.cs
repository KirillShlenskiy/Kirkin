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
    public interface IFastProperty
    {
        /// <summary>
        /// Property specified when this instance was created.
        /// </summary>
        PropertyInfo Property { get; }

        /// <summary>
        /// Invokes the property getter.
        /// </summary>
        object GetValue(object instance);

        /// <summary>
        /// Invokes the property setter.
        /// </summary>
        void SetValue(object instance, object value);
    }
}