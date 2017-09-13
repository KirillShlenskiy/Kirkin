// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//                   THIS IS NOT ORIGINAL WORK.
// Interpreted version of Joe Duffy's PtrUtils.SizeOf<T>() from
// https://github.com/joeduffy/slice.net/blob/master/src/PtrUtils.il
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace Kirkin.Memory
{
    /// <summary>
    /// Type size helpers.
    /// </summary>
    internal static class TypeSizeResolver
    {
        private static readonly ConcurrentDictionary<Type, int> KnownTypeSizes = new ConcurrentDictionary<Type, int>();
        private static readonly Type SizeOfTDynamicContainer = BuildDynamicSizeOfTContainerType();

        /// <summary>
        /// Similar to <see cref="System.Runtime.InteropServices.Marshal.SizeOf(Type)"/>,
        /// but works for any type T (including managed types). Original comment (by Joe Duffy):
        /// Computes the size of any type T. This includes managed object types
        /// which C# complains about (because it is architecture dependent).
        /// </summary>
        public static int GetSize(Type type)
        {
            return KnownTypeSizes.TryGetValue(type, out int size)
                ? size
                : GetSizeSlow(type);
        }

        private static int GetSizeSlow(Type type)
        {
            MethodInfo method = SizeOfTDynamicContainer.GetMethod("SizeOf", BindingFlags.Static | BindingFlags.Public);
            MethodInfo genericMethod = method.MakeGenericMethod(type);

            int size = (int)genericMethod.Invoke(null, null);

            return KnownTypeSizes.GetOrAdd(type, size);
        }

        private static Type BuildDynamicSizeOfTContainerType()
        {
            AssemblyName assemblyName = new AssemblyName("Hacks");
            AssemblyBuilder assemBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder modBuilder = assemBuilder.DefineDynamicModule("Hacks");
            TypeBuilder typeBuilder = modBuilder.DefineType("PtrUtils", TypeAttributes.Public | TypeAttributes.Class);

            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                "SizeOf", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(int), null
            );

            GenericTypeParameterBuilder[] genericParams = methodBuilder.DefineGenericParameters("T");
            ILGenerator ilGenerator = methodBuilder.GetILGenerator();

            ilGenerator.Emit(OpCodes.Sizeof, genericParams[0]);
            ilGenerator.Emit(OpCodes.Ret);

            return typeBuilder.CreateType();
        }
    }
}