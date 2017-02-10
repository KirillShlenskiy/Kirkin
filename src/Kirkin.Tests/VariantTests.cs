using System;

using NUnit.Framework;

namespace Kirkin.Tests
{
    public class Variant2Tests
    {
        [Test]
        public void Int32()
        {
            Variant2 v = new Variant2(123);

            //Assert.AreEqual(typeof(int), v.ValueType);
            Assert.AreEqual(123, v.GetValue<int>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<long>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void Int64()
        {
            Variant2 v = new Variant2(123L);

            //Assert.AreEqual(typeof(long), v.ValueType);
            Assert.AreEqual(123L, v.GetValue<long>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<int>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void Float()
        {
            Variant2 v = new Variant2(123f);

            //Assert.AreEqual(typeof(float), v.ValueType);
            Assert.AreEqual(123f, v.GetValue<float>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<int>());

            // TODO: should this throw?
            //Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void Double()
        {
            Variant2 v = new Variant2(123.0);

            //Assert.AreEqual(typeof(double), v.ValueType);
            Assert.AreEqual(123.0, v.GetValue<double>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<float>());

            // TODO: should this throw?
            //Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void DateTime()
        {
            Variant2 v = new Variant2(new DateTime(2017, 01, 01));

            Assert.AreEqual(typeof(DateTime), v.ValueType);
            Assert.AreEqual(new DateTime(2017, 01, 01), v.GetValue<DateTime>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<float>());

            // TODO: should this throw?
            //Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void String()
        {
            Variant2 v = new Variant2("Hello");

            Assert.AreEqual(typeof(string), v.ValueType);
            Assert.AreEqual("Hello", v.GetValue<string>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<int>());

            // TODO: should this throw?
            //Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void ClrType()
        {
            Variant2 v = new Variant2(typeof(int));

            Assert.AreEqual(typeof(Type), v.ValueType);
            Assert.AreEqual(typeof(int), v.GetValue<Type>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<int>());

            // TODO: should this throw?
            //Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void Null()
        {
            Variant2 v = new Variant2(null);

            Assert.AreEqual(typeof(object), v.ValueType);
            Assert.IsNull(v.GetValue<object>());
        }

        [Test]
        public void Int32_Bechnchmark_Boxing()
        {
            object Variant2 = 123;
            int num;

            for (int i = 0; i < 10000000; i++) {
                num = (int)Variant2;
            }
        }

        [Test]
        public void Int32_Bechnchmark_Boxing2()
        {
            int num;

            for (int i = 0; i < 10000000; i++)
            {
                object Variant2 = 123;

                num = (int)Variant2;
            }
        }

        [Test]
        public void Int32_Bechnchmark_Variant2()
        {
            Variant2 Variant2 = new Variant2(123);
            int num;

            for (int i = 0; i < 10000000; i++) {
                num = Variant2.GetValue<int>();
            }
        }

        [Test]
        public void Int32_Bechnchmark_Variant22()
        {
            int num;

            for (int i = 0; i < 10000000; i++)
            {
                Variant2 Variant2 = new Variant2(123);

                num = Variant2.GetValue<int>();
            }
        }

        [Test]
        public unsafe void Variant2WithTypeRef()
        {
            int value = 123;
            TypedReference typeRef = __makeref(value);
            void* address = &value;

            int copy = *(int*)address;

            *((long*)address) = 321;

            //__refvalue(typeRef, int) = 321;

            Assert.AreEqual(321, value);
        }
    }
}