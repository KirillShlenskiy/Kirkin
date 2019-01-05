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
        public static byte[] GenerateKey() => CryptoRandom.GetRandomBytes(KeySizeInBytes);

        internal static int EncryptBytes(ArraySegment<byte> plaintext, byte[] key, byte[] iv, byte[] output, int outputOffset)
        {
            using (ICryptoTransform transform = AES256_CBC_PKCS7.CreateEncryptor(key, iv))
            {
                if (!transform.CanTransformMultipleBlocks) {
                    throw new NotSupportedException("AES encryptor does not support multi-block transforms.");
                }

                int blockCount = plaintext.Count / BlockSizeInBytes + 1;
                int bytesWritten = 0;

                if (blockCount > 1)
                {
                    int count = (blockCount - 1) * BlockSizeInBytes;

                    bytesWritten += transform.TransformBlock(plaintext.Array, plaintext.Offset, count, output, outputOffset);
                }

                int finalBlockIndex = plaintext.Offset + (blockCount - 1) * BlockSizeInBytes;
                byte[] finalBlock = transform.TransformFinalBlock(plaintext.Array, finalBlockIndex, plaintext.Offset + plaintext.Count - finalBlockIndex);

                Array.Copy(finalBlock, 0, output, outputOffset + bytesWritten, finalBlock.Length);

                bytesWritten += finalBlock.Length;

                return bytesWritten;
            }
        }

        internal static int DecryptBytes(ArraySegment<byte> ciphertext, byte[] key, byte[] iv, byte[] output, int outputOffset)
        {
            using (ICryptoTransform transform = AES256_CBC_PKCS7.CreateDecryptor(key, iv))
            {
                if (!transform.CanTransformMultipleBlocks) {
                    throw new NotSupportedException("AES encryptor does not support multi-block transforms.");
                }

                int blockCount = ciphertext.Count / BlockSizeInBytes;
                int bytesWritten = 0;

                if (blockCount > 1)
                {
                    int count = (blockCount - 1) * BlockSizeInBytes;

                    bytesWritten += transform.TransformBlock(ciphertext.Array, ciphertext.Offset, count, output, outputOffset);
                }

                int finalBlockIndex = ciphertext.Offset + (blockCount - 1) * BlockSizeInBytes;
                byte[] finalBlock = transform.TransformFinalBlock(ciphertext.Array, finalBlockIndex, BlockSizeInBytes);

                Array.Copy(finalBlock, 0, output, outputOffset + bytesWritten, finalBlock.Length);

                bytesWritten += finalBlock.Length;

                return bytesWritten;
            }
        }
    }
}