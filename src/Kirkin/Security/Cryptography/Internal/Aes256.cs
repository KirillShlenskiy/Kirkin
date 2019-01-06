using System;
using System.Security.Cryptography;
using System.Threading;

namespace Kirkin.Security.Cryptography.Internal
{
    internal static class Aes256
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
        /// Generates a random 256-bit key which can be used by an <see cref="Aes256Cbc"/> instance.
        /// </summary>
        public static byte[] GenerateKey() => CryptoRandom.GetRandomBytes(KeySizeInBytes);

        internal static int EncryptBytesCbcPkcs7(in ArraySegment<byte> plaintext, byte[] key, byte[] iv, byte[] output, int outputOffset)
        {
            using (ICryptoTransform transform = AES256_CBC_PKCS7.CreateEncryptor(key, iv)) {
                return ApplyTransform(plaintext, transform, output, outputOffset);
            }
        }

        internal static int DecryptBytesCbcPkcs7(in ArraySegment<byte> ciphertext, byte[] key, byte[] iv, byte[] output, int outputOffset)
        {
            using (ICryptoTransform transform = AES256_CBC_PKCS7.CreateDecryptor(key, iv)) {
                return ApplyTransform(ciphertext, transform, output, outputOffset);
            }
        }

        private static int ApplyTransform(in ArraySegment<byte> input, ICryptoTransform transform, byte[] output, int outputOffset)
        {
            int blockCount = input.Count / BlockSizeInBytes;

            if (input.Count % BlockSizeInBytes != 0) {
                blockCount++;
            }

            int bytesWritten = 0;

            if (blockCount > 1 && transform.CanTransformMultipleBlocks)
            {
                int count = (blockCount - 1) * BlockSizeInBytes;

                bytesWritten += transform.TransformBlock(input.Array, input.Offset, count, output, outputOffset);
            }

            int finalBlockIndex = input.Offset + (blockCount - 1) * BlockSizeInBytes;
            byte[] finalBlock = transform.TransformFinalBlock(input.Array, finalBlockIndex, input.Offset + input.Count - finalBlockIndex);

            Array.Copy(finalBlock, 0, output, outputOffset + bytesWritten, finalBlock.Length);

            bytesWritten += finalBlock.Length;

            return bytesWritten;
        }
    }
}