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
#if DEBUG
            [DllImport(@"..\..\..\Debug\Kirkin.Native.dll")]
#else
            [DllImport(@"..\..\..\Release\Kirkin.Native.dll")]
#endif
            public static extern int Add(int a, int b);
        }
    }
}