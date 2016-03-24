using System.Reflection;

namespace Kirkin.Reflection
{
    /// <summary>
    /// Common PropertyInfo extensions.
    /// </summary>
    internal static class PropertyInfoExtensions
    {
        /// <summary>
        /// Determines whether this PropertyInfo
        /// instance describes a static property.
        /// </summary>
        public static bool IsStatic(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.CanRead)
            {
                // Check the getter.
                return propertyInfo.GetGetMethod(true).IsStatic;
            }

            if (propertyInfo.CanWrite)
            {
                // Check the setter.
                return propertyInfo.GetSetMethod(true).IsStatic;
            }

            // Ask the declaring type - slightly slower.
            var staticPropertyInfo = propertyInfo.DeclaringType.GetProperty(
                propertyInfo.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
            );

            return staticPropertyInfo != null;
        }
    }
}