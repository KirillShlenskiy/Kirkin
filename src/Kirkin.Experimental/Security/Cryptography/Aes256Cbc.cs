using System;
using System.Security.Cryptography;
using System.Threading;

namespace Kirkin.Security.Cryptography
{
    internal static class Aes256Cbc
    {
        internal const int KeySizeInBytes = 32;
        internal const int BlockSizeInBytes = 16;

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
        /// Generates a random 256-bit key which can be used by an <see cref="Aes256CbcAlgorithm"/> instance.
        /// </summary>
        public static byte[] GenerateKey() => CryptoRandom.GetRandomBytes(Aes256Cbc.KeySizeInBytes);

        internal static int EncryptBytes(byte[] plaintextBytes, int plaintextOffset, int plaintextCount, byte[] key, byte[] iv, byte[] output, int outputOffset)
        {
            using (ICryptoTransform transform = AES256_CBC_PKCS7.CreateEncryptor(key, iv))
            {
                if (!transform.CanTransformMultipleBlocks) {
                    throw new NotSupportedException("AES encryptor does not support multi-block transforms.");
                }

                int blockCount = plaintextCount / BlockSizeInBytes + 1;
                int bytesWritten = 0;

                if (blockCount > 1)
                {
                    int count = (blockCount - 1) * BlockSizeInBytes;

                    bytesWritten += transform.TransformBlock(plaintextBytes, plaintextOffset, count, output, outputOffset);
                }

                int finalBlockIndex = plaintextOffset + (blockCount - 1) * BlockSizeInBytes;
                byte[] finalBlock = transform.TransformFinalBlock(plaintextBytes, finalBlockIndex, plaintextOffset + plaintextCount - finalBlockIndex);

                Array.Copy(finalBlock, 0, output, outputOffset + bytesWritten, finalBlock.Length);

                bytesWritten += finalBlock.Length;

                return bytesWritten;
            }
        }

        internal static int DecryptBytes(byte[] ciphertextBytes, int ciphertextOffset, int ciphertextCount, byte[] key, byte[] iv, byte[] output, int outputOffset)
        {
            using (ICryptoTransform transform = AES256_CBC_PKCS7.CreateDecryptor(key, iv))
            {
                if (!transform.CanTransformMultipleBlocks) {
                    throw new NotSupportedException("AES encryptor does not support multi-block transforms.");
                }

                int blockCount = ciphertextCount / BlockSizeInBytes;
                int bytesWritten = 0;

                if (blockCount > 1)
                {
                    int count = (blockCount - 1) * BlockSizeInBytes;

                    bytesWritten += transform.TransformBlock(ciphertextBytes, ciphertextOffset, count, output, outputOffset);
                }

                int finalBlockIndex = ciphertextOffset + (blockCount - 1) * BlockSizeInBytes;
                byte[] finalBlock = transform.TransformFinalBlock(ciphertextBytes, finalBlockIndex, BlockSizeInBytes);

                Array.Copy(finalBlock, 0, output, outputOffset + bytesWritten, finalBlock.Length);

                bytesWritten += finalBlock.Length;

                return bytesWritten;
            }
        }
    }
}