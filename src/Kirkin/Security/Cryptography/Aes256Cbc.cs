using System;

using Kirkin.Security.Cryptography.Internal;

namespace Kirkin.Security.Cryptography
{  
    /// <summary>
    /// Symmetric crypto algorithm implementation which uses the AES256 CBC cipher,
    /// PKCS7 padding and prefixes the ciphertext with the random IV in plain text.
    /// </summary>
    public class Aes256Cbc : SymmetricCryptoFormatter
    {
        private byte[] _key;

        /// <summary>
        /// Returns AES block size - 16 bytes (128 bits).
        /// </summary>
        protected internal override int BlockSize => Aes256.BlockSizeInBytes;

        /// <summary>
        /// Returns the IV length - 16 bytes (128 bits).
        /// </summary>
        protected internal override int MessagePrefixLength => Aes256.BlockSizeInBytes;

        /// <summary>
        /// Returns zero (no suffix).
        /// </summary>
        protected internal override int MessageSuffixLength => 0;

        /// <summary>
        /// Copy of the 256-bit encryption key specified when this instance was created.
        /// </summary>
        public byte[] Key
        {
            get
            {
                byte[] keyCopy = new byte[_key.Length];

                Array.Copy(_key, 0, keyCopy, 0, _key.Length);

                return keyCopy;
            }
        }

        /// <summary>
        /// Creates a new <see cref="Aes256Cbc"/> instance with a randomly-generated key.
        /// </summary>
        public Aes256Cbc()
        {
            _key = Aes256.GenerateKey();
        }

        /// <summary>
        /// Creates a new <see cref="Aes256Cbc"/> instance with the given 256-bit key.
        /// </summary>
        public Aes256Cbc(byte[] key)
        {
            if (key.Length != 32) throw new ArgumentException("Invalid key length.");

            byte[] keyCopy = new byte[key.Length];

            Array.Copy(key, 0, keyCopy, 0, key.Length);

            _key = keyCopy;
        }

        /// <summary>
        /// Encrypts the given plaintext bytes.
        /// </summary>
        /// <returns>Number of bytes written to the output buffer.</returns>
        protected internal override int EncryptBytes(in ArraySegment<byte> plaintext, byte[] output, int outputOffset)
        {
            byte[] iv = CryptoRandom.GetRandomBytes(Aes256.BlockSizeInBytes);

            Array.Copy(iv, 0, output, outputOffset, iv.Length);

            int ciphertextBytesWritten = Aes256.EncryptBytesCbcPkcs7(plaintext, Key, iv, output, outputOffset + iv.Length);

            return iv.Length + ciphertextBytesWritten;
        }

        /// <summary>
        /// Decrypts the given ciphertext bytes.
        /// </summary>
        /// <returns>Number of bytes written to the output buffer.</returns>
        protected internal override int DecryptBytes(in ArraySegment<byte> ciphertext, byte[] output, int outputOffset)
        {
            byte[] iv = new byte[Aes256.BlockSizeInBytes];

            Array.Copy(ciphertext.Array, ciphertext.Offset, iv, 0, iv.Length);

            int ciphertextOffset = iv.Length;
            int ciphertextLength = ciphertext.Count - iv.Length;

            ArraySegment<byte> ciphertextSlice = new ArraySegment<byte>(ciphertext.Array, ciphertext.Offset + ciphertextOffset, ciphertextLength);

            return Aes256.DecryptBytesCbcPkcs7(ciphertextSlice, Key, iv, output, outputOffset);
        }

        /// <summary>
        /// Clears the key material and releases resources used by this instance.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_key != null)
            {
                Array.Clear(_key, 0, _key.Length);

                _key = null;
            }
        }
    }
}