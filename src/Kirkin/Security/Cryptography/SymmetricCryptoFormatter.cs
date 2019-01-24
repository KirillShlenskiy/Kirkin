using System;
using System.Text;

using Kirkin.Security.Cryptography.Internal;

namespace Kirkin.Security.Cryptography
{
    /// <summary>
    /// Base class for symmetric encryption message formatters.
    /// </summary>
    public abstract partial class SymmetricCryptoFormatter : IDisposable
    {
        private static readonly Encoding SafeUTF8
            = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        /// <summary>
        /// Block size, in bytes.
        /// </summary>
        protected abstract int BlockSize { get; }

        /// <summary>
        /// Message prefix length, in bytes.
        /// </summary>
        protected abstract int MessagePrefixLength { get; }

        /// <summary>
        /// Message suffix length, in bytes.
        /// </summary>
        protected abstract int MessageSuffixLength { get; }

        /// <summary>
        /// Encrypts the given plaintext bytes.
        /// </summary>
        /// <returns>Number of bytes written to the output buffer.</returns>
        protected internal abstract int EncryptBytes(in ArraySegment<byte> plaintext, byte[] output, int outputOffset);

        /// <summary>
        /// Decrypts the given ciphertext bytes.
        /// </summary>
        /// <returns>Number of bytes written to the output buffer.</returns>
        protected internal abstract int DecryptBytes(in ArraySegment<byte> ciphertextBytes, byte[] output, int outputOffset);

        /// <summary>
        /// Encrypts the given plaintext bytes.
        /// </summary>
        public byte[] EncryptBytes(byte[] plaintextBytes)
        {
            int length = MaxEncryptOutputBufferSize(plaintextBytes);
            byte[] output = new byte[length];
            int resultLength = EncryptBytes(plaintextBytes.AsArraySegment(), output, 0);

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
            int resultLength = DecryptBytes(ciphertextBytes.AsArraySegment(), output, 0);

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

        /// <summary>
        /// Determines the maximum encrypted message length for the given plaintext.
        /// </summary>
        internal int MaxEncryptOutputBufferSize(byte[] plaintextBytes)
        {
            int blockCount = plaintextBytes.Length / BlockSize + 1;

            return MessagePrefixLength + blockCount * BlockSize + MessageSuffixLength; // iv + ciphertext.
        }

        /// <summary>
        /// Determines the maximum decrypted message length for the given ciphertext.
        /// </summary>
        internal int MaxDecryptOutputBufferSize(byte[] ciphertextBytes)
        {
            return ciphertextBytes.Length - MessagePrefixLength - MessageSuffixLength;
        }

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