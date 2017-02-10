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

            Assert.AreEqual(123, v.GetValue<long>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<int>());
            Assert.Throws<InvalidCastException>(() => v.GetValue<object>());
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
    }
}