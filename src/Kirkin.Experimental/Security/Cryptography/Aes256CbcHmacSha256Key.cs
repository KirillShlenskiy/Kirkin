using System;
using System.Security.Cryptography;
using System.Text;

namespace Kirkin.Security.Cryptography
{
    internal sealed class Aes256CbcHmacSha256Key : IDisposable
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

        public Aes256CbcHmacSha256Key(string algorithmName = "AES256_CBC_HMAC_SHA256")
            : this(GenerateMasterKey(), algorithmName)
        {
        }

        public Aes256CbcHmacSha256Key(byte[] masterKey, string algorithmName = "AES256_CBC_HMAC_SHA256")
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

        static byte[] GenerateMasterKey()
        {
            return CryptoRandom.GetRandomBytes(KeySize / 8);
        }

        public void Dispose()
        {
            Array.Clear(EncryptionKey, 0, EncryptionKey.Length);
            Array.Clear(MACKey, 0, MACKey.Length);
        }
    }
}