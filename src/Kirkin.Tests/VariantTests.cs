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

            Assert.AreEqual(123, v.GetValue<int>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<long>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void Int64()
        {
            Variant v = new Variant(123L);

            Assert.AreEqual(123L, v.GetValue<long>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<int>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void Float()
        {
            Variant v = new Variant(123f);

            Assert.AreEqual(123f, v.GetValue<float>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<int>());

            // TODO: should this throw?
            //Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void Double()
        {
            Variant v = new Variant(123.0);

            Assert.AreEqual(123.0, v.GetValue<double>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<float>());

            // TODO: should this throw?
            //Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void DateTime()
        {
            Variant v = new Variant(new DateTime(2017, 01, 01));

            Assert.AreEqual(new DateTime(2017, 01, 01), v.GetValue<DateTime>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<float>());

            // TODO: should this throw?
            //Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void String()
        {
            Variant v = new Variant("Hello");

            Assert.AreEqual("Hello", v.GetValue<string>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<int>());

            // TODO: should this throw?
            //Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }

        [Test]
        public void ClrType()
        {
            Variant v = new Variant(typeof(int));

            Assert.AreEqual(typeof(int), v.GetValue<Type>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<int>());

            // TODO: should this throw?
            //Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
        }
    }
}