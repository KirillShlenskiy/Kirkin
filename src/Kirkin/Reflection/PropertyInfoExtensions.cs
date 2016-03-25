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
            // Check the getter.
            if (propertyInfo.CanRead) {
                return propertyInfo.GetGetMethod(nonPublic: true).IsStatic;
            }

            // Check the setter.
            if (propertyInfo.CanWrite) {
                return propertyInfo.GetSetMethod(nonPublic: true).IsStatic;
            }

            // Ask the declaring type - slightly slower.
            PropertyInfo staticPropertyInfo = propertyInfo.DeclaringType.GetProperty(
                propertyInfo.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
            );

            return staticPropertyInfo != null;
        }
    }
}