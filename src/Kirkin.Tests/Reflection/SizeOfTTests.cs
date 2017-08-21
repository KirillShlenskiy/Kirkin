using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

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

        struct Dummy
        {
            public int ID;
            public string Value;
        }

        [Test]
        public void SizeOfDummy()
        {
            Assert.AreEqual(Marshal.SizeOf(typeof(Dummy)), SizeOfT.Get<Dummy>());
        }

        [Test]
        public void Perf()
        {
            for (int i = 0; i < 1000000; i++) {
                SizeOfT.Get<int>();
            }
        }

        [Test]
        public void PerfMarshal()
        {
            for (int i = 0; i < 1000000; i++) {
                Marshal.SizeOf(typeof(int));
            }
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
                return _Get<T>.Compiled();
            }

            static class _Get<T>
            {
                public static readonly Func<int> Compiled = CompileDelegate();

                private static Func<int> CompileDelegate()
                {
                    MethodInfo method = SizeOfTContainer.GetMethod("SizeOf", BindingFlags.Static | BindingFlags.Public);
                    MethodInfo genericMethod = method.MakeGenericMethod(typeof(T));

                    return (Func<int>)Delegate.CreateDelegate(typeof(Func<int>), genericMethod);
                }
            }
        }
    }
}