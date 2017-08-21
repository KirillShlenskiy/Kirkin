using System;
using System.Reflection;
using System.Reflection.Emit;

using NUnit.Framework;

namespace Kirkin.Tests.Reflection
{
    public class SizeOfTTests
    {
        [Test]
        public void SizeOfInt32()
        {
            Assert.AreEqual(4, SizeOfT.Get<int>());
        }

        // Interpreted version of Joe Duffy's PtrUtils from https://github.com/joeduffy/slice.net/blob/master/src/PtrUtils.il.
        static class SizeOfT
        {
            private static readonly Type SizeOfTContainer = CreateSizeOfTContainer();

            private static Type CreateSizeOfTContainer()
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

            public static int Get<T>()
            {
                MethodInfo method = SizeOfTContainer.GetMethod("SizeOf", BindingFlags.Static | BindingFlags.Public);
                MethodInfo genericMethod = method.MakeGenericMethod(typeof(T));

                return (int)genericMethod.Invoke(null, null);
            }
        }
    }
}