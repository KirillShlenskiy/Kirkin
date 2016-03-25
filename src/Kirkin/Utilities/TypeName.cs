using System;
using System.Text;

namespace Kirkin.Utilities
{
    /// <summary>
    /// Type name utilities.
    /// </summary>
    internal static class TypeName
    {
        /// <summary>
        /// Gets the short (non-qualified) type name, including any generic type arguments.
        /// </summary>
        public static string NameIncludingGenericArguments(Type type)
        {
            if (!type.IsGenericType) {
                return type.Name;
            }

            Type[] genericArgTypes = type.GetGenericArguments();
            StringBuilder sb = new StringBuilder();

            sb.Append(type.Name, 0, type.Name.IndexOf('`'));
            sb.Append('<');

            for (int i = 0; i < genericArgTypes.Length; i++)
            {
                if (i != 0)
                {
                    sb.Append(',');
                    sb.Append(' ');
                }

                sb.Append(NameIncludingGenericArguments(genericArgTypes[i]));
            }

            sb.Append('>');

            return sb.ToString();
        }
    }
}