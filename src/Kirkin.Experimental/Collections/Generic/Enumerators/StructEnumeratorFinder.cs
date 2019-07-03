using System;
using System.Reflection;

namespace Kirkin.Collections.Generic.Enumerators
{
    internal static class StructEnumeratorFinder
    {
        public static MethodInfo FindValueTypeGetEnumeratorMethod(Type type)
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (method.Name == "GetEnumerator" && method.ReturnType.IsValueType)
                {
                    // Confirm duck typing.
                    Type enumeratorType = method.ReturnType;

                    if (enumeratorType.GetProperty("Current") == null) {
                        continue;
                    }

                    MethodInfo moveNextMethod = enumeratorType.GetMethod("MoveNext");

                    if (moveNextMethod == null || moveNextMethod.ReturnType != typeof(bool)) {
                        continue;
                    }

                    return method;
                }
            }

            return null;
        }
    }
}