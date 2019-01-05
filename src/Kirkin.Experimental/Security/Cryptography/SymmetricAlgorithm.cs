using System;
using System.Text;

namespace Kirkin.Security.Cryptography
{
    /// <summary>
    /// Base class for symmetric encryption algorithms.
    /// </summary>
    public abstract class SymmetricAlgorithm : IDisposable
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
        /// Encrypts the given plaintext bytes.
        /// </summary>
        public byte[] EncryptBytes(byte[] plaintextBytes)
        {
            int length = MaxEncryptOutputBufferSize(plaintextBytes);
            byte[] output = new byte[length];
            int resultLength = EncryptBytes(plaintextBytes, output);

            if (resultLength != output.Length) {
                Array.Resize(ref output, resultLength);
            }

            return output;
        }

        /// <summary>
        /// Encrypts the given plaintext bytes.
        /// </summary>
        public byte[] EncryptString(string plaintextString)
        {
            byte[] plaintextBytes = SafeUTF8.GetBytes(plaintextString);

            return EncryptBytes(plaintextBytes);
        }

        /// <summary>
        /// Decrypts the given ciphertext bytes.
        /// </summary>
        public byte[] DecryptBytes(byte[] ciphertextBytes)
        {
            int length = MaxDecryptOutputBufferSize(ciphertextBytes);
            byte[] output = new byte[length];
            int resultLength = DecryptBytes(ciphertextBytes, output);

            if (resultLength != output.Length) {
                Array.Resize(ref output, resultLength);
            }

            return output;
        }

        /// <summary>
        /// Decrypts the given ciphertext bytes.
        /// </summary>
        public string DecryptString(byte[] ciphertextBytes)
        {
            byte[] plaintextBytes = DecryptBytes(ciphertextBytes);

            return SafeUTF8.GetString(plaintextBytes);
        }

        protected internal abstract int EncryptBytes(byte[] plaintextBytes, byte[] output);
        protected internal abstract int DecryptBytes(byte[] ciphertextBytes, byte[] output);
        protected internal abstract int MaxEncryptOutputBufferSize(byte[] plaintextBytes);
        protected internal abstract int MaxDecryptOutputBufferSize(byte[] ciphertextBytes);

        /// <summary>
        /// Releases the resources used by this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases the resources used by this instance.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}