using System.Reflection;

namespace Kirkin.Reflection
{
    /// <summary>
    /// Provides fast access to property getter and setter methods.
    /// </summary>
    public interface IPropertyAccessor
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