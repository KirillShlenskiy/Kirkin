using System;
using System.Linq.Expressions;
using System.Reflection.Emit;

using NUnit.Framework;

namespace Kirkin.Tests.Experimental
{
    public class ObjectFactoryBenchmarks
    {
        public class DummyClass
        {
        }

        public struct DummyStruct
        {
        }

        static ObjectFactoryBenchmarks()
        {
            CreateFactoryViaDynamicMethod<DummyClass>();
            CreateFactoryViaExpression<DummyClass>();
        }

        #region Tests

        [Test]
        public void ExpressionFactoryClass()
        {
            for (int i = 0; i < 500; i++)
            {
                Func<DummyClass> factory = CreateFactoryViaExpression<DummyClass>();
                DummyClass instance = factory.Invoke();

                Assert.NotNull(instance);
            }
        }

        [Test]
        public void ExpressionFactoryStruct()
        {
            for (int i = 0; i < 500; i++)
            {
                Func<DummyStruct> factory = CreateFactoryViaExpression<DummyStruct>();
                DummyStruct instance = factory.Invoke();

                Assert.NotNull(instance);
            }
        }

        [Test]
        public void DynamicMethodFactoryClass()
        {
            for (int i = 0; i < 500; i++)
            {
                Func<DummyClass> factory = CreateFactoryViaDynamicMethod<DummyClass>();
                DummyClass instance = factory.Invoke();

                Assert.NotNull(instance);
            }
        }

        [Test]
        public void DynamicMethodFactoryStruct()
        {
            for (int i = 0; i < 500; i++)
            {
                Func<DummyStruct> factory = CreateFactoryViaDynamicMethod<DummyStruct>();
                DummyStruct instance = factory.Invoke();

                Assert.NotNull(instance);
            }
        }

        #endregion

        #region Implementation

        private static Func<T> CreateFactoryViaExpression<T>()
        {
            NewExpression newExpr = Expression.New(typeof(T));

            return Expression
                .Lambda<Func<T>>(newExpr)
                .Compile();
        }

        private static Func<T> CreateFactoryViaDynamicMethod<T>()
        {
            DynamicMethod method = new DynamicMethod("CreateInstance", typeof(T), null);

            // Emit "return new T()".
            ILGenerator ilGenerator = method.GetILGenerator();

            if (typeof(T).IsValueType)
            {
                ilGenerator.DeclareLocal(typeof(T)); // Local index 0.

                ilGenerator.Emit(OpCodes.Ldloca, 0);
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

        #endregion
    }
}
