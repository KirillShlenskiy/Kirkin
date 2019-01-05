using System;
using System.Security.Cryptography;
using System.Threading;

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

        private static readonly ThreadLocal<Aes> s_aes256_cbc_pkcs7 = new ThreadLocal<Aes>(() =>
        {
            Aes aes = CryptoFactories.AesFactory();

            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            return aes;
        });

        /// <summary>
        /// Shared thread local AES256 CBC PKCS7 instance.
        /// </summary>
        internal static Aes AES256_CBC_PKCS7 => s_aes256_cbc_pkcs7.Value;

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

            using (ICryptoTransform transform = AES256_CBC_PKCS7.CreateEncryptor(Key, iv))
            {
                if (!transform.CanTransformMultipleBlocks) {
                    throw new NotSupportedException("AES encryptor does not support multi-block transforms.");
                }

                int blockCount = plaintextBytes.Length / blockSizeInBytes + 1;
                int outputOffset = iv.Length;

                if (blockCount > 1)
                {
                    int count = (blockCount - 1) * blockSizeInBytes;

                    outputOffset += transform.TransformBlock(plaintextBytes, 0, count, output, outputOffset);
                }

                int finalBlockIndex = (blockCount - 1) * blockSizeInBytes;
                byte[] finalBlock = transform.TransformFinalBlock(plaintextBytes, finalBlockIndex, plaintextBytes.Length - finalBlockIndex);

                Array.Copy(finalBlock, 0, output, outputOffset, finalBlock.Length);

                return outputOffset + finalBlock.Length;
            }
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

            using (ICryptoTransform transform = AES256_CBC_PKCS7.CreateDecryptor(Key, iv))
            {
                if (!transform.CanTransformMultipleBlocks) {
                    throw new NotSupportedException("AES encryptor does not support multi-block transforms.");
                }

                int blockCount = (ciphertextBytes.Length - iv.Length) / blockSizeInBytes;
                int outputOffset = 0;

                if (blockCount > 1)
                {
                    int count = (blockCount - 1) * blockSizeInBytes;

                    outputOffset = transform.TransformBlock(ciphertextBytes, iv.Length, count, output, 0);
                }

                byte[] finalBlock = transform.TransformFinalBlock(ciphertextBytes, ciphertextBytes.Length - blockSizeInBytes, blockSizeInBytes);

                Array.Copy(finalBlock, 0, output, outputOffset, finalBlock.Length);

                return outputOffset + finalBlock.Length;
            }
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