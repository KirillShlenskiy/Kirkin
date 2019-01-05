using System;

namespace Kirkin.Security.Cryptography
{  
    /// <summary>
    /// Symmetric crypto algorithm implementation which uses the AES256 CBC cipher,
    /// PKCS7 padding and prefixes the ciphertext with the random IV in plain text.
    /// </summary>
    public sealed class Aes256CbcAlgorithm : SymmetricAlgorithm
    {
        /// <summary>
        /// Generates a random 256-bit key which can be used by an <see cref="Aes256CbcAlgorithm"/> instance.
        /// </summary>
        public static byte[] GenerateKey() => CryptoRandom.GetRandomBytes(32);

        /// <summary>
        /// 256 bits/32 bytes (AES 256).
        /// </summary>
        public override int KeySize => 256;

        /// <summary>
        /// 128 bit/16 bytes (AES standard).
        /// </summary>
        public override int BlockSize => 128;

        /// <summary>
        /// Encryption key specified when this instance was created.
        /// </summary>
        public byte[] Key { get; }

        /// <summary>
        /// Creates a new <see cref="Aes256CbcAlgorithm"/> instance with a randomly-generated key.
        /// </summary>
        public Aes256CbcAlgorithm()
        {
            Key = GenerateKey();
        }

        /// <summary>
        /// Creates a new <see cref="Aes256CbcAlgorithm"/> instance with the given key.
        /// </summary>
        public Aes256CbcAlgorithm(byte[] key)
        {
            if (key.Length != 32) throw new ArgumentException("Invalid key length.");

            byte[] keyCopy = new byte[key.Length];

            Array.Copy(key, 0, keyCopy, 0, key.Length);

            Key = keyCopy;
        }

        /// <summary>
        /// Encrypts the given plaintext bytes.
        /// </summary>
        /// <returns>Number of bytes written to the output buffer.</returns>
        protected internal override int EncryptBytes(byte[] plaintextBytes, byte[] output)
        {
            int blockSizeInBytes = BlockSize / 8;
            byte[] iv = CryptoRandom.GetRandomBytes(blockSizeInBytes);

            Array.Copy(iv, 0, output, 0, iv.Length);

            return Aes256Cbc.EncryptBytes(plaintextBytes, Key, iv, output, iv.Length);
        }

        /// <summary>
        /// Decrypts the given ciphertext bytes.
        /// </summary>
        /// <returns>Number of bytes written to the output buffer.</returns>
        protected internal override int DecryptBytes(byte[] ciphertextBytes, byte[] output)
        {
            int blockSizeInBytes = BlockSize / 8;
            byte[] iv = new byte[blockSizeInBytes];

            Array.Copy(ciphertextBytes, 0, iv, 0, iv.Length);

            return Aes256Cbc.DecryptBytes(ciphertextBytes, Key, iv, output, 0);
        }

        protected internal override int MaxEncryptOutputBufferSize(byte[] plaintextBytes)
        {
            int blockSizeInBytes = BlockSize / 8;
            int ivLength = blockSizeInBytes;
            int blockCount = plaintextBytes.Length / blockSizeInBytes + 1;

            return ivLength + blockCount * blockSizeInBytes; // iv + ciphertext.
        }

        protected internal override int MaxDecryptOutputBufferSize(byte[] ciphertextBytes)
        {
            int ivLength = BlockSize / 8;

            return ciphertextBytes.Length - ivLength;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Array.Clear(Key, 0, Key.Length);
        }
    }
}