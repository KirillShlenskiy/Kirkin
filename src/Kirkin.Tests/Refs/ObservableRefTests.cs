using Kirkin.Refs;

using Xunit;

namespace Kirkin.Tests.Refs
{
    public class ObservableRefTests
    {
        [Fact]
        public void ValueSetAndValueChanged()
        {
            ObservableRef<int> strongRef = new ObservableRef<int>(123);

            int valueSetCount = 0;
            int valueChangedCount = 0;

            strongRef.ValueSet += (s, e) => valueSetCount++;
            strongRef.ValueChanged += (s, e) => valueChangedCount++;

            strongRef.Value = 321;

            Assert.Equal(1, valueSetCount);
            Assert.Equal(1, valueChangedCount);

            strongRef.Value = 321;

            Assert.Equal(2, valueSetCount);
            Assert.Equal(1, valueChangedCount);

            strongRef.Value++;

            Assert.Equal(3, valueSetCount);
            Assert.Equal(2, valueChangedCount);
        }
    }
}