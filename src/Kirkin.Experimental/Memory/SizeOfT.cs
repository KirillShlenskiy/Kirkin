// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//                   THIS IS NOT ORIGINAL WORK.
// Interpreted version of Joe Duffy's PtrUtils.SizeOf<T>() from
// https://github.com/joeduffy/slice.net/blob/master/src/PtrUtils.il
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Kirkin.Memory
{
    /// <summary>
    /// Type size helpers.
    /// </summary>
    internal static class SizeOfT
    {
        private static readonly Type SizeOfTDynamicContainer = BuildDynamicSizeOfTContainerType();

        /// <summary>
        /// Similar to <see cref="System.Runtime.InteropServices.Marshal.SizeOf(Type)"/>, but works on managed types too.
        /// Original comment (by Joe Duffy):
        /// Computes the size of any type T. This includes managed object types
        /// which C# complains about (because it is architecture dependent).
        /// </summary>
        public static int Get<T>()
        {
            return _Get<T>.Compiled();
        }

        static class _Get<T>
        {
            public static readonly Func<int> Compiled = CompileDelegate();

            private static Func<int> CompileDelegate()
            {
                MethodInfo method = SizeOfTDynamicContainer.GetMethod("SizeOf", BindingFlags.Static | BindingFlags.Public);
                MethodInfo genericMethod = method.MakeGenericMethod(typeof(T));

                return (Func<int>)Delegate.CreateDelegate(typeof(Func<int>), genericMethod);
            }
        }

        private static Type BuildDynamicSizeOfTContainerType()
        {
            AppDomain domain = AppDomain.CurrentDomain;
            AssemblyName assemblyName = new AssemblyName("Hacks");
            AssemblyBuilder assemBuilder = domain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
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