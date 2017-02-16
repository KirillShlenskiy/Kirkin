using System;

namespace Kirkin.Cryptography
{
    internal static class CryptoKernelExtensions
    {
        /// <summary>
        /// Encrypts the plain text input using the given secret.
        /// </summary>
        internal static string EncryptBase64(this ICryptoKernel kernel, string plainText, string secret)
        {
            if (string.IsNullOrEmpty(plainText)) throw new ArgumentException(nameof(plainText));
            if (string.IsNullOrEmpty(secret)) throw new ArgumentException(nameof(secret));

            return Convert.ToBase64String(kernel.Encrypt(plainText, secret));
        }

        /// <summary>
        /// Decrypts the encrypted text using the provided secret.
        /// </summary>
        internal static string DecryptBase64(this ICryptoKernel kernel, string encryptedText, string secret)
        {
            if (string.IsNullOrEmpty(encryptedText)) throw new ArgumentException(nameof(encryptedText));
            if (string.IsNullOrEmpty(secret)) throw new ArgumentException(nameof(secret));

            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

            return kernel.Decrypt(encryptedBytes, secret);
        }
    }
}