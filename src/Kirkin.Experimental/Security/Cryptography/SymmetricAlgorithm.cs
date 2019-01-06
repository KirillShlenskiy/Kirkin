using System;
using System.Security.Cryptography;
using System.Text;

using Kirkin.Security.Cryptography.Internal;

namespace Kirkin.Security.Cryptography
{
    /// <summary>
    /// Base class for symmetric encryption algorithms.
    /// </summary>
    public abstract partial class SymmetricAlgorithm : IDisposable
    {
        private static readonly Encoding SafeUTF8
            = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

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
        /// Creates a <see cref="ICryptoTransform"/> which can be used to encrypt streams.
        /// </summary>
        internal ICryptoTransform CreateEncryptor()
        {
            return new SymmetricAlgorithmEncryptTransform(this);
        }

        /// <summary>
        /// Creates a <see cref="ICryptoTransform"/> which can be used to decrypt streams.
        /// </summary>
        internal ICryptoTransform CreateDecryptor()
        {
            return new SymmetricAlgorithmDecryptTransform(this);
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

        protected internal abstract int EncryptBytes(in ArraySegment<byte> plaintext, byte[] output, int outputOffset);
        protected internal abstract int DecryptBytes(in ArraySegment<byte> ciphertextBytes, byte[] output, int outputOffset);
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