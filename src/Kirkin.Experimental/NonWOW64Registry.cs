using System;

using Microsoft.Win32;

namespace Kirkin
{
    /// <summary>
    /// Registry access proxy which does not use WOW6432Node even if running in a 32-bit process.
    /// </summary>
    public static class NonWOW64Registry
    {
        private readonly static object _lock = new object();

        private static RegistryKey _currentUser;
        private static RegistryKey _localMachine;

        public static RegistryKey CurrentUser
        {
            get
            {
                if (_currentUser == null)
                {
                    lock (_lock)
                    {
                        if (_currentUser == null)
                        {
                            RegistryView view = RegistryView.Default;

                            if (Environment.Is64BitOperatingSystem)
                                view = RegistryView.Registry64;// Force non-WOW6432Node access even if running as x86.

                            _currentUser = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view);
                        }
                    }
                }

                return _currentUser;
            }
        }

        public static RegistryKey LocalMachine
        {
            get
            {
                if (_localMachine == null)
                {
                    lock (_lock)
                    {
                        if (_localMachine == null)
                        {
                            RegistryView view = RegistryView.Default;

                            if (Environment.Is64BitOperatingSystem)
                                view = RegistryView.Registry64;// Force non-WOW6432Node access even if running as x86.

                            _localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                        }
                    }
                }

                return _localMachine;
            }
        }

        public static object GetValue(string keyName, string valueName, object defaultValue)
        {
            string hiveName = HiveNameFromFullPath(keyName);
            RegistryKey baseKey = BaseKeyFromHive(GetRegistryHive(hiveName));
            string subKeyName = SubKeyNameFromFullPath(keyName);

            using (RegistryKey key = baseKey.OpenSubKey(subKeyName, writable: false))
            {
                // Preserve My.Computer.Registry.GetValue semantics: return Nothing
                // (instead of the defaultValue provided) if the key does not exist.
                return key?.GetValue(valueName, defaultValue);
            }
        }

        public static void SetValue(string keyName, string valueName, object value, RegistryValueKind valueKind)
        {
            string hiveName = HiveNameFromFullPath(keyName);
            RegistryKey baseKey = BaseKeyFromHive(GetRegistryHive(hiveName));
            string subKeyName = SubKeyNameFromFullPath(keyName);
            RegistryKey key = null;

            try
            {
                key = baseKey.OpenSubKey(subKeyName, writable: true);

                if (key == null)
                    key = baseKey.CreateSubKey(subKeyName);

                key.SetValue(valueName, value);
            }
            finally
            {
                key?.Dispose();
            }
        }

        private static RegistryKey BaseKeyFromHive(RegistryHive hive)
        {
            if (hive == RegistryHive.LocalMachine) return LocalMachine;
            if (hive == RegistryHive.CurrentUser) return CurrentUser;

            throw new ArgumentException($"Unhandled registry hive: '{hive}'.");
        }

        private static string HiveNameFromFullPath(string registryPath)
        {
            int firstDelimiterIndex = registryPath.IndexOf('\\');

            if (firstDelimiterIndex == -1) {
                throw new FormatException("Malformed registry path.");
            }

            return registryPath.Substring(0, firstDelimiterIndex);
        }

        private static string SubKeyNameFromFullPath(string registryPath)
        {
            int firstDelimiterIndex = registryPath.IndexOf('\\');

            if (firstDelimiterIndex == -1) {
                throw new FormatException("Malformed registry path.");
            }

            return registryPath.Substring(firstDelimiterIndex + 1);
        }

        private static RegistryHive GetRegistryHive(string name)
        {
            if (string.Equals(name, "HKEY_LOCAL_MACHINE", StringComparison.OrdinalIgnoreCase)) return RegistryHive.LocalMachine;
            if (string.Equals(name, "HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase)) return RegistryHive.CurrentUser;

            throw new ArgumentException($"Unknown registry hive: '{name}'.");
        }
    }
}