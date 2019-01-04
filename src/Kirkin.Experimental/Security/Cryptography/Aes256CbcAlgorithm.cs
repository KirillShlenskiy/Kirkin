﻿using System;
using System.Security.Cryptography;

namespace Kirkin.Security.Cryptography
{
    /// <summary>
    /// Symmetric crypto algorithm implementation which uses the AES256 CBC cipher,
    /// PKCS7 padding and prefixes the ciphertext with the random IV in plain text.
    /// </summary>
    public sealed class Aes256CbcAlgorithm : SymmetricAlgorithm
    {
        /// <summary>
        /// 256 bits/32 bytes (AES 256).
        /// </summary>
        public override int KeySize => 256;

        /// <summary>
        /// 128 bit/16 bytes (AES standard).
        /// </summary>
        public override int BlockSize => 128;

        /// <summary>
        /// Encrypts the given plaintext bytes using the given key.
        /// </summary>
        /// <returns>Number of bytes written to the output buffer.</returns>
        protected internal override int EncryptBytes(byte[] plaintextBytes, byte[] key, byte[] output)
        {
            int blockSizeInBytes = BlockSize / 8;
            byte[] iv = new byte[blockSizeInBytes];

            // Reuse?
            using (RandomNumberGenerator rng = CryptoFactories.RngFactory()) {
                rng.GetBytes(iv);
            }

            Array.Copy(iv, 0, output, 0, iv.Length);

            Aes aes = BorrowAes();

            try
            {
                using (ICryptoTransform transform = aes.CreateEncryptor(key, iv))
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
            finally
            {
                ReturnAes(aes);
            }
        }

        /// <summary>
        /// Decrypts the given ciphertext bytes using the given key.
        /// </summary>
        /// <returns>Number of bytes written to the output buffer.</returns>
        protected internal override int DecryptBytes(byte[] ciphertextBytes, byte[] key, byte[] output)
        {
            int blockSizeInBytes = BlockSize / 8;
            byte[] iv = new byte[blockSizeInBytes];

            Array.Copy(ciphertextBytes, 0, iv, 0, iv.Length);

            Aes aes = BorrowAes();

            try
            {
                using (ICryptoTransform transform = aes.CreateDecryptor(key, iv))
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
            finally
            {
                ReturnAes(aes);
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

        private static Aes BorrowAes()
        {
            Aes aes = CryptoFactories.AesFactory();

            if (aes.Mode != CipherMode.CBC || aes.Padding != PaddingMode.PKCS7)
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
            }

            return aes;
        }

        private static void ReturnAes(Aes aes)
        {
            aes.Dispose();
        }
    }
}