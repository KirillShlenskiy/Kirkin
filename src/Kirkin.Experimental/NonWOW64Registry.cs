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

        /// <summary>
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
        /// HKEY_CLASSES_ROOT base key.
        /// </summary>
        public static RegistryKey ClassesRoot
        {
            get
            {
                if (_classesRoot == null)
                {
                    lock (_lock)
                    {
                        if (_classesRoot == null) {
                            _classesRoot = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView);
                        }
                    }
                }

                return _classesRoot;
            }
        }

        /// <summary>
        /// HKEY_USERS base key.
        /// </summary>
        public static RegistryKey Users
        {
            get
            {
                if (_users == null)
                {
                    lock (_lock)
                    {
                        if (_users == null) {
                            _users = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView);
                        }
                    }
                }

                return _users;
            }
        }

        /// <summary>
        /// HKEY_PERFORMANCE_DATA base key.
        /// </summary>
        public static RegistryKey PerformanceData
        {
            get
            {
                if (_performanceData == null)
                {
                    lock (_lock)
                    {
                        if (_performanceData == null) {
                            _performanceData = RegistryKey.OpenBaseKey(RegistryHive.PerformanceData, RegistryView);
                        }
                    }
                }

                return _performanceData;
            }
        }

        /// <summary>
        /// HKEY_CURRENT_CONFIG base key.
        /// </summary>
        public static RegistryKey CurrentConfig
        {
            get
            {
                if (_currentConfig == null)
                {
                    lock (_lock)
                    {
                        if (_currentConfig == null) {
                            _currentConfig = RegistryKey.OpenBaseKey(RegistryHive.CurrentConfig, RegistryView);
                        }
                    }
                }

                return _currentConfig;
            }
        }

        /// <summary>
        /// HKEY_CURRENT_CONFIG base key.
        /// </summary>
        public static RegistryKey DynData
        {
            get
            {
                if (_dynData == null)
                {
                    lock (_lock)
                    {
                        if (_dynData == null) {
                            _dynData = RegistryKey.OpenBaseKey(RegistryHive.DynData, RegistryView);
                        }
                    }
                }

                return _dynData;
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