using System;

using NUnit.Framework;

namespace Kirkin.Tests
{
    public class VariantTests
    {
        [Test]
        public void Int32()
        {
            Variant v = new Variant(123);

            //Assert.AreEqual(typeof(int), v.ValueType);
            Assert.AreEqual(123, v.GetValue<int>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<long>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void Int64()
        {
            Variant v = new Variant(123L);

            //Assert.AreEqual(typeof(long), v.ValueType);
            Assert.AreEqual(123L, v.GetValue<long>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<int>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void Float()
        {
            Variant v = new Variant(123f);

            //Assert.AreEqual(typeof(float), v.ValueType);
            Assert.AreEqual(123f, v.GetValue<float>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<int>());

            // TODO: should this throw?
            //Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void Double()
        {
            Variant v = new Variant(123.0);

            //Assert.AreEqual(typeof(double), v.ValueType);
            Assert.AreEqual(123.0, v.GetValue<double>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<float>());

            // TODO: should this throw?
            //Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void DateTime()
        {
            Variant v = new Variant(new DateTime(2017, 01, 01));

            Assert.AreEqual(typeof(DateTime), v.ValueType);
            Assert.AreEqual(new DateTime(2017, 01, 01), v.GetValue<DateTime>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<float>());

            // TODO: should this throw?
            //Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void String()
        {
            Variant v = new Variant("Hello");

            Assert.AreEqual(typeof(string), v.ValueType);
            Assert.AreEqual("Hello", v.GetValue<string>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<int>());

            // TODO: should this throw?
            //Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void ClrType()
        {
            Variant v = new Variant(typeof(int));

            Assert.AreEqual(typeof(Type), v.ValueType);
            Assert.AreEqual(typeof(int), v.GetValue<Type>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<int>());

            // TODO: should this throw?
            //Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void Null()
        {
            Variant v = new Variant(null);

            Assert.AreEqual(typeof(object), v.ValueType);
            Assert.IsNull(v.GetValue<object>());
        }

        [Test]
        public void Int32_Bechnchmark_Boxing()
        {
            object v = 123;
            int num;

            for (int i = 0; i < 10000000; i++) {
                num = (int)v;
            }
        }

        [Test]
        public void Int32_Bechnchmark_Boxing2()
        {
            int num;

            for (int i = 0; i < 10000000; i++)
            {
                object v = 123;

                num = (int)v;
            }
        }

        [Test]
        public void Int32_Bechnchmark_Variant()
        {
            Variant v = new Variant(123);
            int num;

            for (int i = 0; i < 10000000; i++) {
                num = v.GetValue<int>();
            }
        }

        [Test]
        public void Int32_Bechnchmark_Variant2()
        {
            int num;

            for (int i = 0; i < 10000000; i++)
            {
                Variant v = new Variant(123);

                num = v.GetValue<int>();
            }
        }

        [Test]
        public unsafe void CopyViaPointer()
        {
            int value = 123;
            TypedReference typeRef = __makeref(value);
            void* address = &value;
            int copy = *(int*)address;

            *((int*)address) = 321;

            Assert.AreEqual(321, value);
        }

        [Test]
        public unsafe void CopyViaTypeRef()
        {
            Container<int> container = new Container<int>();

            Assert.AreEqual(0, container.Value);

            TypedReference containerRef = __makeref(container);

            // The first field of TypedReference is an IntPtr value pointer.
            int* containerPtr = (int*)*((IntPtr*)&containerRef);

            *containerPtr = 123;

            Assert.AreEqual(123, container.Value);
        }

        struct Container<T>
        {
            internal T Value;
        }
    }
}