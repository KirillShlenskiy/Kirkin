using NUnit.Framework;

namespace Kirkin
{
    public class VariantArrayTests
    {
        [Test]
        public void BasicIntegerVariantArray()
        {
            VariantArray arr = new VariantArray(8);

            arr.SetInt(0, int.MaxValue);
            arr.SetInt(4, int.MinValue);

            Assert.AreEqual(int.MaxValue, arr.GetInt(0));
            Assert.AreEqual(int.MinValue, arr.GetInt(4));
        }

        [Test]
        public void BasicLongVariantArray()
        {
            VariantArray arr = new VariantArray(16);

            arr.SetLong(0, (long)int.MaxValue + 1);
            arr.SetLong(8, (long)int.MinValue - 1);

            Assert.AreEqual((long)int.MaxValue + 1, arr.GetLong(0));
            Assert.AreEqual((long)int.MinValue - 1, arr.GetLong(8));
        }
    }
}