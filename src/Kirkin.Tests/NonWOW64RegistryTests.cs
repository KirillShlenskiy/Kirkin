using System;

using Microsoft.Win32;

using NUnit.Framework;

namespace Kirkin.Tests
{
    public class NonWOW64RegistryTests
    {
        [Test]
        public void NonWow64Write()
        {
            if (Environment.Is64BitProcess) {
                Assert.Ignore("Suppressed on x64.");
            }

            string keyName = Guid.NewGuid().ToString().Replace("-", "");
            RegistryKey hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);

            Assert.Null(hkcu.OpenSubKey($@"Software\Kirkin\{keyName}"));

            RegistryKey key = hkcu.CreateSubKey($@"Software\Kirkin\{keyName}");

            key.SetValue("Test", "Test");

            try
            {
                Assert.AreEqual("Test", (string)NonWOW64Registry.GetValue($@"HKEY_CURRENT_USER\Software\Kirkin\{keyName}", "Test", ""));
                Assert.True(string.IsNullOrEmpty((string)Registry.GetValue($@"HKEY_CURRENT_USER\Software\Kirkin\{keyName}", "Test", "")));
            }
            finally
            {
                hkcu.DeleteSubKeyTree($@"Software\Kirkin\{keyName}");
            }
        }
    }
}