using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace Kirkin.Tests.Native
{
    public class NativeTests
    {
#if DEBUG
        const string DLL_PATH = @"..\..\..\Debug\Kirkin.Native.dll";
#else
        const string DLL_PATH = @"..\..\..\Release\Kirkin.Native.dll";
#endif

        [Test]
        public void NativeAdd()
        {
            SkipTestIfNativeDllDoesntExist();

            Assert.AreEqual(3, NativeMethods.Add(1, 2));
        }

        private static void SkipTestIfNativeDllDoesntExist()
        {
            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            string root = Path.GetDirectoryName(assembly.Location);
            string dllPath = Path.Combine(root, DLL_PATH);

            if (!File.Exists(dllPath)) {
                throw new IgnoreException("Kirkin.Native.dll does not exist.");
            }
        }

        static class NativeMethods
        {
            [DllImport(DLL_PATH)]
            public static extern int Add(int a, int b);
        }
    }
}