using System;

using Kirkin.Refs;

using Xunit;

namespace Kirkin.Tests.Refs
{
    public class RefTests
    {
        [Fact]
        public void PropertyRef()
        {
            Dummy dummy = new Dummy { ID = 123 };
            ValueRef<int> id = ValueRef.FromExpression(() => dummy.ID);

            Assert.Equal(123, id.Value);

            id.Value = 321;

            Assert.Equal(321, dummy.ID);

            IRef weakID = id;

            Assert.Equal(321, weakID.Value);

            weakID.Value = 111;

            Assert.Equal(111, dummy.ID);
            Assert.ThrowsAny<Exception>(() => weakID.Value = "222");
        }

        [Fact]
        public void LocalRef()
        {
            int value = 123;
            ValueRef<int> valueRef = ValueRef.FromExpression(() => value);

            Assert.Equal(123, valueRef.Value);

            valueRef.Value = 321;

            Assert.Equal(321, value);

            IRef weakID = valueRef;

            Assert.Equal(321, weakID.Value);

            weakID.Value = 111;

            Assert.Equal(111, value);
            Assert.ThrowsAny<Exception>(() => weakID.Value = "222");
        }

        sealed class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }
    }
}