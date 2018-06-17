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
        private static RegistryKey _classesRoot;
        private static RegistryKey _users;
        private static RegistryKey _performanceData;
        private static RegistryKey _currentConfig;
        private static RegistryKey _dynData;

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
        public static RegistryKey CurrentUser => GetBaseKey(ref _currentUser, RegistryHive.CurrentUser);

        /// <summary>
        /// HKEY_LOCAL_MACHINE base key.
        /// </summary>
        public static RegistryKey LocalMachine => GetBaseKey(ref _localMachine, RegistryHive.LocalMachine);

        /// <summary>
        /// HKEY_CLASSES_ROOT base key.
        /// </summary>
        public static RegistryKey ClassesRoot => GetBaseKey(ref _classesRoot, RegistryHive.ClassesRoot);

        /// <summary>
        /// HKEY_USERS base key.
        /// </summary>
        public static RegistryKey Users => GetBaseKey(ref _users, RegistryHive.Users);

        /// <summary>
        /// HKEY_PERFORMANCE_DATA base key.
        /// </summary>
        public static RegistryKey PerformanceData => GetBaseKey(ref _performanceData, RegistryHive.PerformanceData);

        /// <summary>
        /// HKEY_CURRENT_CONFIG base key.
        /// </summary>
        public static RegistryKey CurrentConfig => GetBaseKey(ref _currentConfig, RegistryHive.CurrentConfig);

        /// <summary>
        /// HKEY_CURRENT_CONFIG base key.
        /// </summary>
        public static RegistryKey DynData => GetBaseKey(ref _dynData, RegistryHive.DynData);

        /// <summary>
        /// Gets a value from a registry key.
        /// </summary>
        public static object GetValue(string keyName, string valueName, object defaultValue)
        {
            SplitPath(keyName, out string hiveName, out string subKeyName);

            RegistryKey baseKey = BaseKeyFromHive(GetRegistryHive(hiveName));

            using (RegistryKey key = baseKey.OpenSubKey(subKeyName, writable: false))
            {
                // VB.Net My.Computer.Registry.GetValue semantics: return null
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

        private static RegistryKey GetBaseKey(ref RegistryKey location, RegistryHive hive)
        {
            if (location == null)
            {
                lock (_lock)
                {
                    if (location == null) {
                        location = RegistryKey.OpenBaseKey(hive, RegistryView);
                    }
                }
            }

            return location;
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
            if (string.Equals(name, "HKEY_CLASSES_ROOT", StringComparison.OrdinalIgnoreCase)) return RegistryHive.ClassesRoot;
            if (string.Equals(name, "HKEY_USERS", StringComparison.OrdinalIgnoreCase)) return RegistryHive.Users;
            if (string.Equals(name, "HKEY_PERFORMANCE_DATA", StringComparison.OrdinalIgnoreCase)) return RegistryHive.PerformanceData;
            if (string.Equals(name, "HKEY_CURRENT_CONFIG", StringComparison.OrdinalIgnoreCase)) return RegistryHive.CurrentConfig;
            if (string.Equals(name, "HKEY_DYN_DATA", StringComparison.OrdinalIgnoreCase)) return RegistryHive.DynData;

            throw new ArgumentException($"Unknown registry hive: '{name}'.");
        }
    }
}