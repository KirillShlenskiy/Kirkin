﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Kirkin.Cryptography
{
    /// <summary>
    /// Simple AesManaged wrapper.
    /// </summary>
    /// <remarks>
    /// Originally based on Encryptamajig (MIT license - https://github.com/jbubriski/Encryptamajig).
    /// </remarks>
    public sealed class AES256Encryption : ICryptoKernel
    {
        private const int KeyBitSize = 256; // AES 256.
        private const int BlockBitSize = 128; // Only valid AES block size.

        private static readonly Encoding SafeUTF8
            = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        /// <summary>
        /// Number of PBKDF2-HMAC-SHA1 iterations used when hashing the secret.
        /// The higher this number, the stronger the encryption. The default is 10,000.
        /// </summary>
        public int HashIterations { get; }

        /// <summary>
        /// Creates a new instance of the AES256 crypto engine.
        /// </summary>
        /// <param name="hashIterations">
        /// Number of PBKDF2-HMAC-SHA1 iterations used when hashing the secret.
        /// The higher this number, the stronger the encryption.
        /// </param>
        public AES256Encryption(int hashIterations = 10000)
        {
            if (hashIterations < 1) throw new ArgumentOutOfRangeException(nameof(hashIterations));

            HashIterations = hashIterations;
        }

        /// <summary>
        /// Encrypts the plain text input using the given secret.
        /// </summary>
        /// <param name="plainText">Plain text to be encrypted.</param>
        /// <param name="secret">Secret passphrase used to encrypt plain text.</param>
        public byte[] Encrypt(string plainText, string secret)
        {
            if (plainText == null) throw new ArgumentNullException(nameof(plainText));
            if (secret == null) throw new ArgumentNullException(nameof(secret));

            const int saltBitSize = 256; // May change in future.

            // Rfc2898DeriveBytes always uses UTF8 no BOM.
            using (Rfc2898DeriveBytes keyDerivationFunction = new Rfc2898DeriveBytes(secret, saltBitSize / 8, HashIterations))
            {
                byte[] saltBytes = keyDerivationFunction.Salt;
                byte[] keyBytes = keyDerivationFunction.GetBytes(KeyBitSize / 8);

                // The default Cipher Mode is CBC and the Padding is PKCS7 which are both good.
                using (AesManaged aes = new AesManaged { KeySize = KeyBitSize, BlockSize = BlockBitSize })
                {
                    byte[] ivBytes = aes.IV;

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (ICryptoTransform encryptor = aes.CreateEncryptor(keyBytes, ivBytes))
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream, SafeUTF8)) {
                            streamWriter.Write(plainText);
                        }

                        byte[] encryptedTextBytes = memoryStream.ToArray();

                        // Result format: 32 bits of salt bit size, 32 bits of SHA1 iteration count,
                        // 256 bits of salt, 128 bits of IV, 128 (or more) bits of encrypted text.
                        byte[] finalResult = new byte[
                            sizeof(int) + sizeof(int) + saltBytes.Length + ivBytes.Length + encryptedTextBytes.Length
                        ];

                        int offset = 0;

                        void WriteResult(byte[] arr)
                        {
                            Array.Copy(arr, 0, finalResult, offset, arr.Length);
                            offset += arr.Length;
                        }

                        WriteResult(BitConverter.GetBytes(saltBitSize));
                        WriteResult(BitConverter.GetBytes(HashIterations));
                        WriteResult(saltBytes);
                        WriteResult(ivBytes);
                        WriteResult(encryptedTextBytes);

                        return finalResult;
                    }
                }
            }
        }

        /// <summary>
        /// Decrypts the byte array using the provided secret.
        /// </summary>
        public string Decrypt(byte[] encryptedBytes, string secret)
        {
            if (encryptedBytes == null) throw new ArgumentNullException(nameof(encryptedBytes));
            if (secret == null) throw new ArgumentNullException(nameof(secret));

            // Input format: 32 bits of salt bit size, 32 bits of SHA1 iteration count,
            // N bits of salt, 128 bits of IV, 128 (or more) bits of encrypted text.
            int saltBitSize = BitConverter.ToInt32(encryptedBytes, 0);
            int hashIterations = BitConverter.ToInt32(encryptedBytes, 4);

            byte[] saltBytes = new byte[saltBitSize / 8];
            byte[] ivBytes = new byte[BlockBitSize / 8];
            byte[] encryptedTextBytes = new byte[encryptedBytes.Length - 8 - saltBytes.Length - ivBytes.Length];

            Array.Copy(encryptedBytes, 8, saltBytes, 0, saltBytes.Length);
            Array.Copy(encryptedBytes, 8 + saltBytes.Length, ivBytes, 0, ivBytes.Length);
            Array.Copy(encryptedBytes, 8 + saltBytes.Length + ivBytes.Length, encryptedTextBytes, 0, encryptedTextBytes.Length);

            // Rfc2898DeriveBytes always uses UTF8 no BOM.
            using (Rfc2898DeriveBytes keyDerivationFunction = new Rfc2898DeriveBytes(secret, saltBytes, hashIterations))
            {
                byte[] keyBytes = keyDerivationFunction.GetBytes(KeyBitSize / 8);

                using (AesManaged aes = new AesManaged { KeySize = KeyBitSize, BlockSize = BlockBitSize })
                using (ICryptoTransform decryptor = aes.CreateDecryptor(keyBytes, ivBytes))
                using (MemoryStream memoryStream = new MemoryStream(encryptedTextBytes))
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                using (StreamReader streamReader = new StreamReader(cryptoStream, SafeUTF8)) {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}