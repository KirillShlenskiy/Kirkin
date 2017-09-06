using System.Runtime.InteropServices;

using NUnit.Framework;

namespace Kirkin.Tests.Native
{
    public class NativeTests
    {
        [Test]
        public void NativeAdd()
        {
            Assert.AreEqual(3, NativeMethods.Add(1, 2));
        }

        static class NativeMethods
        {
            [DllImport(@"..\..\..\Debug\Kirkin.Native.dll")]
            public static extern int Add(int a, int b);
        }
    }
}