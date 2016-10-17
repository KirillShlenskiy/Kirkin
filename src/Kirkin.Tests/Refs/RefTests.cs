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
            IRef<int> id = Ref.FromExpression(() => dummy.ID);

            Assert.Equal(123, id.Value);

            id.Value = 321;

            Assert.Equal(321, dummy.ID);

            IRef weakID = id;

            Assert.Equal(321, weakID.Value);

            weakID.Value = 111;

            Assert.Equal(111, dummy.ID);
            Assert.ThrowsAny<Exception>(() => weakID.Value = "222");
        }

        sealed class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }
    }
}