using Xunit;

using Kirkin.Functional;

namespace Kirkin.Tests.Functional
{
    public class OptionTests
    {
        [Fact]
        public void Projections()
        {
            Option<string> str = null; // "zzz";

            var a = str.Select(_ => "ignored") || null || "ooo";

            Assert.Equal("ooo", a.Value);

            var i = str.Select(_ => 1);

            Assert.False(i.HasValue);
        }

        [Fact]
        public void Equals()
        {
            var o1 = new Option<int>(123);
            var o2 = new Option<int>(321);

            Assert.NotEqual(o1, o2);

            o2 = new Option<int>(123);

            Assert.Equal(o1, o2);
            Assert.Equal(123, o1);

            // Null checking.
            Assert.NotNull(o1);
            Assert.True(new Option<int>().Equals(null));

            // Need to work out why this is failing.
            //Assert.Equal(null, default(Option<int>));
        }

        [Fact]
        public void GetHashCodeT()
        {
            Assert.Equal(1, new Option<int>(1).GetHashCode());
            Assert.Equal(0, new Option<int>().GetHashCode());
        }

        [Fact]
        public void HasValueMustBeTrue()
        {
            Option<string> op0 = "";
            Assert.True(op0.HasValue, "HasValue must be true after implicit conversion from a valid non-null value.");
        }

        [Fact]
        public void HasValueMustBeFalse()
        {
            Option<string> op0 = Option<string>.None;
            Assert.False(op0.HasValue, "HasValue must be false for Option<T>.None.");

            Option<string> op1 = new Option<string>();
            Assert.False(op1.HasValue, "HasValue must be false for default(Option<T>).");

            Option<string> op2 = null;
            Assert.True(op2 == null);
            Assert.False(op2.HasValue, "HasValue must be true after implicit conversion from a null reference.");
        }

        [Fact]
        public void NullCoalescingOperatos()
        {
            var i = new Option<int>();

            Assert.Equal(1, i | 1);
            Assert.Equal(2, i || 2);
            Assert.Equal(3, i % 3);

            Assert.NotEqual(0, i | 1);
            Assert.NotEqual(0, i || 2);
            Assert.NotEqual(0, i % 3);
        }

        [Fact]
        public void ValueTypes()
        {
            Option<int> z = 0;

            Assert.True(z.HasValue);
        }
    }
}