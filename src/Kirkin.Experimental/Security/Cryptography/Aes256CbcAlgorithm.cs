using System;

namespace Kirkin.Security.Cryptography
{  
    /// <summary>
    /// Symmetric crypto algorithm implementation which uses the AES256 CBC cipher,
    /// PKCS7 padding and prefixes the ciphertext with the random IV in plain text.
    /// </summary>
    public class Aes256CbcAlgorithm : SymmetricAlgorithm
    {
        private byte[] _key;

        /// <summary>
        /// 256 bits/32 bytes (AES 256).
        /// </summary>
        public override int KeySize => Aes256Cbc.KeySizeInBytes * 8;

        /// <summary>
        /// 128 bit/16 bytes (AES standard).
        /// </summary>
        public override int BlockSize => Aes256Cbc.BlockSizeInBytes * 8;

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
        /// Creates a new <see cref="Aes256CbcAlgorithm"/> instance with a randomly-generated key.
        /// </summary>
        public Aes256CbcAlgorithm()
        {
            _key = Aes256Cbc.GenerateKey();
        }

        /// <summary>
        /// Creates a new <see cref="Aes256CbcAlgorithm"/> instance with the given key.
        /// </summary>
        public Aes256CbcAlgorithm(byte[] key)
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
            byte[] iv = CryptoRandom.GetRandomBytes(Aes256Cbc.BlockSizeInBytes);

            Array.Copy(iv, 0, output, outputOffset, iv.Length);

            int ciphertextBytesWritten = Aes256Cbc.EncryptBytes(plaintext, Key, iv, output, outputOffset + iv.Length);

            return iv.Length + ciphertextBytesWritten;
        }

        /// <summary>
        /// Decrypts the given ciphertext bytes.
        /// </summary>
        /// <returns>Number of bytes written to the output buffer.</returns>
        protected internal override int DecryptBytes(in ArraySegment<byte> ciphertext, byte[] output, int outputOffset)
        {
            byte[] iv = new byte[Aes256Cbc.BlockSizeInBytes];

            Array.Copy(ciphertext.Array, ciphertext.Offset, iv, 0, iv.Length);

            int ciphertextOffset = iv.Length;
            int ciphertextLength = ciphertext.Count - iv.Length;

            ArraySegment<byte> ciphertextSlice = new ArraySegment<byte>(ciphertext.Array, ciphertext.Offset + ciphertextOffset, ciphertextLength);

            return Aes256Cbc.DecryptBytes(ciphertextSlice, Key, iv, output, outputOffset);
        }

        protected internal override int MaxEncryptOutputBufferSize(byte[] plaintextBytes)
        {
            int ivLength = Aes256Cbc.BlockSizeInBytes;
            int blockCount = plaintextBytes.Length / Aes256Cbc.BlockSizeInBytes + 1;

            return ivLength + blockCount * Aes256Cbc.BlockSizeInBytes; // iv + ciphertext.
        }

        protected internal override int MaxDecryptOutputBufferSize(byte[] ciphertextBytes)
        {
            int ivLength = Aes256Cbc.BlockSizeInBytes;

            return ciphertextBytes.Length - ivLength;
        }

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