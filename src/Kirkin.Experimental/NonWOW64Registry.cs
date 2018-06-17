using System;

using Microsoft.Win32;

namespace Kirkin
{
    /// <summary>
    /// Registry proxy which does not use WOW6432Node even if running in a 32-bit process.
    /// </summary>
    public static class NonWOW64Registry
    {
        private readonly static object _lock = new object();

        private static RegistryKey _currentUser;
        private static RegistryKey _localMachine;

        private static RegistryView RegistryView
        {
            get
            {
                return Environment.Is64BitOperatingSystem
                    ? RegistryView.Registry64
                    : RegistryView.Default;
            }
        }

        /// <summary>
        /// HKEY_CURRENT_USER base key.
        /// </summary>
        public static RegistryKey CurrentUser
        {
            get
            {
                if (_currentUser == null)
                {
                    lock (_lock)
                    {
                        if (_currentUser == null) {
                            _currentUser = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView);
                        }
                    }
                }

                return _currentUser;
            }
        }

        // <summary>
        /// HKEY_LOCAL_MACHINE base key.
        /// </summary>
        public static RegistryKey LocalMachine
        {
            get
            {
                if (_localMachine == null)
                {
                    lock (_lock)
                    {
                        if (_localMachine == null) {
                            _localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView);
                        }
                    }
                }

                return _localMachine;
            }
        }

        /// <summary>
        /// Gets a value from a registry key.
        /// </summary>
        public static object GetValue(string keyName, string valueName, object defaultValue)
        {
            SplitPath(keyName, out string hiveName, out string subKeyName);

            RegistryKey baseKey = BaseKeyFromHive(GetRegistryHive(hiveName));

            using (RegistryKey key = baseKey.OpenSubKey(subKeyName, writable: false))
            {
                // Preserve My.Computer.Registry.GetValue semantics: return Nothing
                // (instead of the defaultValue provided) if the key does not exist.
                return key?.GetValue(valueName, defaultValue);
            }
        }

        /// <summary>
        /// Writes a value to a registry key.
        /// </summary>
        public static void SetValue(string keyName, string valueName, object value)
        {
            SetValue(keyName, valueName, value, RegistryValueKind.Unknown);
        }

        /// <summary>
        /// Writes a value to a registry key.
        /// </summary>
        public static void SetValue(string keyName, string valueName, object value, RegistryValueKind valueKind)
        {
            SplitPath(keyName, out string hiveName, out string subKeyName);

            RegistryKey baseKey = BaseKeyFromHive(GetRegistryHive(hiveName));

            using (RegistryKey key = baseKey.OpenSubKey(subKeyName, writable: true) ?? baseKey.CreateSubKey(subKeyName)) {
                key.SetValue(valueName, value);
            }
        }

        private static void SplitPath(string registryPath, out string hiveName, out string subKeyName)
        {
            int firstDelimiterIndex = registryPath.IndexOf('\\');

            if (firstDelimiterIndex == -1) {
                throw new FormatException("Malformed registry path.");
            }

            hiveName = registryPath.Substring(0, firstDelimiterIndex);
            subKeyName = registryPath.Substring(firstDelimiterIndex + 1);
        }

        private static RegistryKey BaseKeyFromHive(RegistryHive hive)
        {
            if (hive == RegistryHive.LocalMachine) return LocalMachine;
            if (hive == RegistryHive.CurrentUser) return CurrentUser;

            throw new ArgumentException($"Unhandled registry hive: '{hive}'.");
        }

        private static RegistryHive GetRegistryHive(string name)
        {
            if (string.Equals(name, "HKEY_LOCAL_MACHINE", StringComparison.OrdinalIgnoreCase)) return RegistryHive.LocalMachine;
            if (string.Equals(name, "HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase)) return RegistryHive.CurrentUser;

            throw new ArgumentException($"Unknown registry hive: '{name}'.");
        }
    }
}