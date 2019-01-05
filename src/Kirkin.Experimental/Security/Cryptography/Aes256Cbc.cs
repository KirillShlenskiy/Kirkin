using System;
using System.Security.Cryptography;
using System.Threading;

namespace Kirkin.Security.Cryptography
{
    internal static class Aes256Cbc
    {
        private const int BlockSizeInBytes = 16;

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

        internal static int EncryptBytes(byte[] plaintextBytes, byte[] key, byte[] iv, byte[] output, int outputOffset)
        {
            using (ICryptoTransform transform = AES256_CBC_PKCS7.CreateEncryptor(key, iv))
            {
                if (!transform.CanTransformMultipleBlocks) {
                    throw new NotSupportedException("AES encryptor does not support multi-block transforms.");
                }

                int blockCount = plaintextBytes.Length / BlockSizeInBytes + 1;

                if (blockCount > 1)
                {
                    int count = (blockCount - 1) * BlockSizeInBytes;

                    outputOffset += transform.TransformBlock(plaintextBytes, 0, count, output, outputOffset);
                }

                int finalBlockIndex = (blockCount - 1) * BlockSizeInBytes;
                byte[] finalBlock = transform.TransformFinalBlock(plaintextBytes, finalBlockIndex, plaintextBytes.Length - finalBlockIndex);

                Array.Copy(finalBlock, 0, output, outputOffset, finalBlock.Length);

                return outputOffset + finalBlock.Length;
            }
        }

        internal static int DecryptBytes(byte[] ciphertextBytes, byte[] key, byte[] iv, byte[] output, int outputOffset)
        {
            using (ICryptoTransform transform = AES256_CBC_PKCS7.CreateDecryptor(key, iv))
            {
                if (!transform.CanTransformMultipleBlocks) {
                    throw new NotSupportedException("AES encryptor does not support multi-block transforms.");
                }

                int blockCount = (ciphertextBytes.Length - iv.Length) / BlockSizeInBytes;

                if (blockCount > 1)
                {
                    int count = (blockCount - 1) * BlockSizeInBytes;

                    outputOffset += transform.TransformBlock(ciphertextBytes, iv.Length, count, output, 0);
                }

                byte[] finalBlock = transform.TransformFinalBlock(ciphertextBytes, ciphertextBytes.Length - BlockSizeInBytes, BlockSizeInBytes);

                Array.Copy(finalBlock, 0, output, outputOffset, finalBlock.Length);

                return outputOffset + finalBlock.Length;
            }
        }
    }
}