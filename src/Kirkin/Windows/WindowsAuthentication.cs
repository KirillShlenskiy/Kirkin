#if !__MOBILE__ && !NETSTANDARD2_0 && !NET_40

using System;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;

using Kirkin.Collections.Generic;

namespace Kirkin.Windows
{
    /// <summary>
    /// Win32 auth API.
    /// </summary>
    public static class WindowsAuthentication
    {
        /// <summary>
        /// Authenticates the local or domain user.
        /// </summary>
        public static GenericPrincipal Authenticate(string userName, string password)
        {
            string domainName = null;
            int delimiterIndex = userName.IndexOf('\\');

            if (delimiterIndex != -1)
            {
                domainName = userName.Substring(0, delimiterIndex);
                userName = userName.Substring(delimiterIndex + 1);
            }

            IntPtr token = IntPtr.Zero;

            try
            {
                if (!ADVAPI32.LogonUserEx(userName, domainName, password, LOGON32_LOGON_NETWORK, LOGON32_PROVIDER_DEFAULT, out token, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero)) {
                    throw new SecurityException("Authentication failed.", new Win32Exception());
                }

                using (WindowsIdentity windowsIdentity = new WindowsIdentity(token)) {
                    return GenericPrincipalFromWindowsIdentity(windowsIdentity);
                }
            }
            finally
            {
                if (token != IntPtr.Zero) {
                    Kernel32.CloseHandle(token);
                }
            }
        }

        static GenericPrincipal GenericPrincipalFromWindowsIdentity(WindowsIdentity windowsIdentity)
        {
            GenericIdentity identity = new GenericIdentity(windowsIdentity.Name);
            ArrayBuilder<string> groupNames = new ArrayBuilder<string>();

            if (windowsIdentity.Groups != null)
            {
                foreach (IdentityReference group in windowsIdentity.Groups)
                {
                    string groupName = group.Translate(typeof(NTAccount)).Value;

                    groupNames.Add(groupName);
                }
            }

            return new GenericPrincipal(identity, groupNames.ToArray());
        }

        #region Platform Invoke

        const int LOGON32_LOGON_NETWORK = 3;
        const int LOGON32_PROVIDER_DEFAULT = 0;

        static class ADVAPI32
        {
            [DllImport("advapi32.dll", SetLastError = true)]
            internal static extern bool LogonUserEx(
                string lpszUsername,
                string lpszDomain,
                string lpszPassword,
                int dwLogonType,
                int dwLogonProvider,
                out IntPtr phToken,
                IntPtr ppLogonSid,
                IntPtr ppProfileBuffer,
                IntPtr pdwProfileLength,
                IntPtr pQuotaLimits
            );
        }

        static class Kernel32
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CloseHandle(IntPtr handle);
        }

        #endregion
    }
}

#endif