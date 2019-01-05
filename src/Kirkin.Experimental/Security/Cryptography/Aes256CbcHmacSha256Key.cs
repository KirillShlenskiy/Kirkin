using System;
using System.Security.Cryptography;
using System.Text;

namespace Kirkin.Security.Cryptography
{
    /// <summary>
    /// Key derivation mechanism which produces a 256-bit AES encryption
    /// key and a 256-bit MAC key from a single 256-bit master key.
    /// </summary>
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
        public byte[] EncryptionKey { get; private set; }

        /// <summary>
        /// 256-bit key used for MAC encryption.
        /// </summary>
        public byte[] MACKey { get; private set; }

        public Aes256CbcHmacSha256Key(string algorithmName = "AES256_CBC_HMAC_SHA256")
            : this(GenerateMasterKey(), algorithmName)
        {
        }

        public Aes256CbcHmacSha256Key(byte[] masterKey, string algorithmName = "AES256_CBC_HMAC_SHA256")
        {
            if (masterKey == null) throw new ArgumentNullException(nameof(masterKey));
            if (masterKey.Length != Aes256Cbc.KeySizeInBytes) throw new ArgumentException("Invalid master key length.");
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
            return CryptoRandom.GetRandomBytes(Aes256Cbc.KeySizeInBytes);
        }

        public void Dispose()
        {
            Array.Clear(EncryptionKey, 0, EncryptionKey.Length);
            EncryptionKey = null;

            Array.Clear(MACKey, 0, MACKey.Length);
            MACKey = null;
        }
    }
}