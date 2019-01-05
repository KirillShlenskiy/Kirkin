using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Kirkin.Security.Cryptography
{  
    /// <summary>
    /// Symmetric crypto algorithm implementation which uses the AES256 CBC cipher,
    /// PKCS7 padding and prefixes the ciphertext with the random IV in plain text.
    /// </summary>
    public sealed class Aes256CbcHmacSha256Algorithm : Aes256CbcAlgorithm
    {
        private const int MAC_LENGTH_IN_BYTES = 32; // 256 bits.

        /// <summary>
        /// Creates a new <see cref="Aes256CbcAlgorithm"/> instance with a randomly-generated key.
        /// </summary>
        public Aes256CbcHmacSha256Algorithm()
            : base()
        {
        }

        /// <summary>
        /// Creates a new <see cref="Aes256CbcAlgorithm"/> instance with the given key.
        /// </summary>
        public Aes256CbcHmacSha256Algorithm(byte[] key)
            : base(key)
        {
        }

        /// <summary>
        /// Encrypts the given plaintext bytes.
        /// </summary>
        /// <returns>Number of bytes written to the output buffer.</returns>
        protected internal override int EncryptBytes(byte[] plaintextBytes, byte[] output, int outputOffset)
        {
            int blockSizeInBytes = BlockSize / 8;
            byte[] iv = CryptoRandom.GetRandomBytes(blockSizeInBytes);

            // Write IV.
            Array.Copy(iv, 0, output, 0, iv.Length);

            int bytesWritten = iv.Length;
            byte[] hash;

            using (Aes256CbcHmacSha256Key derivedKey = new Aes256CbcHmacSha256Key(Key))
            {
                // Write ciphertext.
                bytesWritten += Aes256Cbc.EncryptBytes(plaintextBytes, 0, plaintextBytes.Length, derivedKey.EncryptionKey, iv, output, iv.Length);

                // MAC of the IV + ciphertext portion.
                using (HMACSHA256 hmac = new HMACSHA256(derivedKey.MACKey)) {
                    hash = hmac.ComputeHash(output, outputOffset, bytesWritten);
                }
            }

            Array.Copy(hash, 0, output, bytesWritten, hash.Length);

            return bytesWritten + MAC_LENGTH_IN_BYTES;
        }

        /// <summary>
        /// Decrypts the given ciphertext bytes.
        /// </summary>
        /// <returns>Number of bytes written to the output buffer.</returns>
        protected internal override int DecryptBytes(byte[] ciphertextBytes, byte[] output, int outputOffset)
        {
            int blockSizeInBytes = BlockSize / 8;
            byte[] iv = new byte[blockSizeInBytes];
            byte[] expectedHash = new byte[MAC_LENGTH_IN_BYTES];

            Array.Copy(ciphertextBytes, 0, iv, 0, iv.Length);
            Array.Copy(ciphertextBytes, ciphertextBytes.Length - MAC_LENGTH_IN_BYTES, expectedHash, 0, MAC_LENGTH_IN_BYTES);

            using (Aes256CbcHmacSha256Key derivedKey = new Aes256CbcHmacSha256Key(Key))
            {
                byte[] actualHash;

                // MAC of the IV + ciphertext portion.
                using (HMACSHA256 hmac = new HMACSHA256(derivedKey.MACKey)) {
                    actualHash = hmac.ComputeHash(ciphertextBytes, 0, ciphertextBytes.Length - MAC_LENGTH_IN_BYTES);
                }

                if (!((IStructuralEquatable)expectedHash).Equals(actualHash, EqualityComparer<byte>.Default)) {
                    throw new ArgumentException("MAC validation failed.");
                }

                return Aes256Cbc.DecryptBytes(ciphertextBytes, iv.Length, ciphertextBytes.Length - iv.Length - MAC_LENGTH_IN_BYTES, derivedKey.EncryptionKey, iv, output, 0);
            }
        }

        protected internal override int MaxEncryptOutputBufferSize(byte[] plaintextBytes)
        {
            return base.MaxEncryptOutputBufferSize(plaintextBytes) + MAC_LENGTH_IN_BYTES; // iv + ciphertext + MAC.
        }

        protected internal override int MaxDecryptOutputBufferSize(byte[] ciphertextBytes)
        {
            return base.MaxDecryptOutputBufferSize(ciphertextBytes) - MAC_LENGTH_IN_BYTES; // iv + ciphertext + MAC.
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Array.Clear(Key, 0, Key.Length);
        }
    }
}