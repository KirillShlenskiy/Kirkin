using System;
using System.Text;

namespace Kirkin.Security.Cryptography
{
    /// <summary>
    /// Base class for symmetric encryption algorithms.
    /// </summary>
    public abstract class SymmetricAlgorithm
    {
        private static readonly Encoding SafeUTF8
            = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        /// <summary>
        /// Key size, in bits.
        /// </summary>
        public abstract int KeySize { get; }

        /// <summary>
        /// Block size, in bits.
        /// </summary>
        public abstract int BlockSize { get; }

        /// <summary>
        /// Encrypts the given plaintext bytes using the given key.
        /// </summary>
        public byte[] EncryptBytes(byte[] plaintextBytes, byte[] key)
        {
            int length = MaxEncryptOutputBufferSize(plaintextBytes);
            byte[] output = new byte[length];
            int resultLength = EncryptBytes(plaintextBytes, key, output);

            if (resultLength != output.Length) {
                Array.Resize(ref output, resultLength);
            }

            return output;
        }

        /// <summary>
        /// Encrypts the given plaintext bytes using the given key.
        /// </summary>
        public byte[] EncryptString(string plaintextString, byte[] key)
        {
            byte[] plaintextBytes = SafeUTF8.GetBytes(plaintextString);

            return EncryptBytes(plaintextBytes, key);
        }

        /// <summary>
        /// Decrypts the given ciphertext bytes using the given key.
        /// </summary>
        public byte[] DecryptBytes(byte[] ciphertextBytes, byte[] key)
        {
            int length = MaxDecryptOutputBufferSize(ciphertextBytes);
            byte[] output = new byte[length];
            int resultLength = DecryptBytes(ciphertextBytes, key, output);

            if (resultLength != output.Length) {
                Array.Resize(ref output, resultLength);
            }

            return output;
        }

        /// <summary>
        /// Decrypts the given ciphertext bytes using the given key.
        /// </summary>
        public string DecryptString(byte[] ciphertextBytes, byte[] key)
        {
            byte[] plaintextBytes = DecryptBytes(ciphertextBytes, key);

            return SafeUTF8.GetString(plaintextBytes);
        }

        protected internal abstract int EncryptBytes(byte[] plaintextBytes, byte[] key, byte[] output);
        protected internal abstract int DecryptBytes(byte[] ciphertextBytes, byte[] key, byte[] output);
        protected internal abstract int MaxEncryptOutputBufferSize(byte[] plaintextBytes);
        protected internal abstract int MaxDecryptOutputBufferSize(byte[] ciphertextBytes);
    }
}