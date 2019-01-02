using System;
using System.Security.Cryptography;
using System.Text;

namespace Kirkin.Security.Cryptography
{
    internal sealed class HmacSha256EncryptionMacKey
    {
        /// <summary>
        /// Key length in bits.
        /// </summary>
        public const int KeySize = 256;

        /// <summary>
        /// 256-bit master key supplied when this instance was created.
        /// </summary>
        public byte[] MasterKey { get; }

        /// <summary>
        /// 256-bit key used for plaintext encryption.
        /// </summary>
        public byte[] EncryptionKey { get; }

        /// <summary>
        /// 256-bit key used for MAC encryption.
        /// </summary>
        public byte[] MACKey { get; }

        public HmacSha256EncryptionMacKey(byte[] masterKey, string algorithmName)
        {
            if (masterKey == null) throw new ArgumentNullException(nameof(masterKey));
            if (masterKey.Length != KeySize / 8) throw new ArgumentException("Invalid master key length.");
            if (string.IsNullOrEmpty(algorithmName)) throw new ArgumentException("Algorithm name cannot be empty.");

            MasterKey = masterKey;

            byte[] encryptionKey;
            byte[] macKey;

            using (HMACSHA256 hmac = new HMACSHA256(masterKey))
            {
                encryptionKey = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{algorithmName} encryption key derived from master key of length {masterKey.Length}"));
                macKey = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{algorithmName} MAC key derived from master key of length {masterKey.Length}"));
            }

            if (encryptionKey.Length != masterKey.Length || macKey.Length != masterKey.Length) {
                throw new ArgumentException("Invalid encryption or MAC key length.");
            }

            EncryptionKey = encryptionKey;
            MACKey = macKey;
        }
    }
}