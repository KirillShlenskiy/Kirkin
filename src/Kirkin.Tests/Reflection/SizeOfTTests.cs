using System.Runtime.InteropServices;

using Kirkin.Memory;

using NUnit.Framework;

namespace Kirkin.Tests.Reflection
{
    public class SizeOfTTests
    {
        [Test]
        public void SizeOfInt32()
        {
            Assert.AreEqual(4, RawCopy.SizeOf<int>());
            Assert.AreEqual(4, RawCopy.SizeOf(typeof(int)));
        }

        struct Dummy
        {
            public int ID;
            public string Value;
        }

        [Test]
        public void SizeOfDummy()
        {
            Assert.AreEqual(Marshal.SizeOf(typeof(Dummy)), RawCopy.SizeOf<Dummy>());
        }

        [Test]
        public void Perf() // 21
        {
            for (int i = 0; i < 1000000; i++) {
                RawCopy.SizeOf(typeof(int));
            }
        }

        [Test]
        public void PerfMarshal()
        {
            for (int i = 0; i < 1000000; i++) {
                Marshal.SizeOf(typeof(int));
            }
        }
    }
}