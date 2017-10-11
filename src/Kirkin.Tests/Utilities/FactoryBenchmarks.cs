using System;
using System.Linq.Expressions;
using System.Reflection.Emit;

using NUnit.Framework;

namespace Kirkin.Tests.Utilities
{
    public class FactoryBenchmarks
    {
        public class DummyClass
        {
        }

        public struct DummyStruct
        {
        }

        static FactoryBenchmarks()
        {
            CreateFactoryViaDynamicMethod<DummyClass>();
            CreateFactoryViaExpression<DummyClass>();
        }

        [Test]
        public void ExpressionFactory()
        {
            for (int i = 0; i < 500; i++)
            {
                Func<DummyClass> factory = CreateFactoryViaExpression<DummyClass>();
                DummyClass instance = factory.Invoke();

                Assert.NotNull(instance);
            }
        }

        private static Func<T> CreateFactoryViaExpression<T>()
        {
            NewExpression newExpr = Expression.New(typeof(T));

            return Expression
                .Lambda<Func<T>>(newExpr)
                .Compile();
        }

        [Test]
        public void DynamicMethodFactory()
        {
            for (int i = 0; i < 500; i++)
            {
                Func<DummyClass> factory = CreateFactoryViaDynamicMethod<DummyClass>();
                DummyClass instance = factory.Invoke();

                Assert.NotNull(instance);
            }
        }

        private static Func<T> CreateFactoryViaDynamicMethod<T>()
        {
            DynamicMethod method = new DynamicMethod("CreateInstance", typeof(T), null);
            ILGenerator ilGenerator = method.GetILGenerator();

            if (typeof(T).IsValueType)
            {
                ilGenerator.DeclareLocal(typeof(T));
                ilGenerator.Emit(OpCodes.Ldloca_S, 0);
                ilGenerator.Emit(OpCodes.Initobj, typeof(T));
                ilGenerator.Emit(OpCodes.Ldloc_0);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
            }

            ilGenerator.Emit(OpCodes.Ret);

            return (Func<T>)method.CreateDelegate(typeof(Func<T>));
        }
    }
}
