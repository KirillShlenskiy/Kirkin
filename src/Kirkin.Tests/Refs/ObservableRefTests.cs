using Kirkin.Refs;

using NUnit.Framework;

namespace Kirkin.Tests.Refs
{
    public class ObservableRefTests
    {
        [Test]
        public void ValueSetAndValueChanged()
        {
            ObservableRef<int> strongRef = new ObservableRef<int>(123);

            int valueSetCount = 0;
            int valueChangedCount = 0;

            strongRef.ValueSet += (s, e) => valueSetCount++;
            strongRef.ValueChanged += (s, e) => valueChangedCount++;

            strongRef.Value = 321;

            Assert.AreEqual(1, valueSetCount);
            Assert.AreEqual(1, valueChangedCount);

            strongRef.Value = 321;

            Assert.AreEqual(2, valueSetCount);
            Assert.AreEqual(1, valueChangedCount);

            strongRef.Value++;

            Assert.AreEqual(3, valueSetCount);
            Assert.AreEqual(2, valueChangedCount);
        }
    }
}