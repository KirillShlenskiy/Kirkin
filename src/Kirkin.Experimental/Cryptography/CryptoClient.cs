using System;

namespace Kirkin.Cryptography
{
    /// <summary>
    /// Provides convenient string-based wrappers around
    /// <see cref="ICryptoKernel"/> functionla
    /// </summary>
    public sealed class CryptoClient
    {
        public ICryptoKernel Kernel { get; }

        public CryptoClient(ICryptoKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));

            Kernel = kernel;
        }

        /// <summary>
        /// Encrypts the plain text input using the given secret.
        /// </summary>
        public string EncryptBase64(string plainText, string secret)
        {
            if (string.IsNullOrEmpty(plainText)) throw new ArgumentException(nameof(plainText));
            if (string.IsNullOrEmpty(secret)) throw new ArgumentException(nameof(secret));

            return Convert.ToBase64String(Kernel.Encrypt(plainText, secret));
        }

        public string DecryptBase64(string encryptedText, string secret)
        {
            if (string.IsNullOrEmpty(encryptedText)) throw new ArgumentException(nameof(encryptedText));
            if (string.IsNullOrEmpty(secret)) throw new ArgumentException(nameof(secret));

            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

            return Kernel.Decrypt(encryptedBytes, secret);
        }
    }
}