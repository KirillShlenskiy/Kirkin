using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

using Kirkin.Security.Cryptography;

using NUnit.Framework;

using SymmetricAlgorithm = Kirkin.Security.Cryptography.SymmetricAlgorithm;

namespace Kirkin.Tests.Security.Cryptography
{
    public class SymmetricAlgorithmTests
    {
        [Test]
        public void Aes256CbcAlgorithmMultiLength()
        {
            using (Aes256CbcAlgorithm aes = new Aes256CbcAlgorithm())
            {
                for (int i = 1; i < 256; i++)
                {
                    byte[] plaintext = Enumerable.Range(0, i).Select(n => (byte)n).ToArray();
                    byte[] ciphertext = aes.EncryptBytes(plaintext);

                    Assert.AreNotEqual(plaintext, ciphertext);

                    using (AesCryptoServiceProvider provider = new AesCryptoServiceProvider())
                    {
                        byte[] iv = ciphertext.Take(Aes256Cbc.BlockSizeInBytes).ToArray();
                        byte[] cipher = ciphertext.Skip(iv.Length).ToArray();

                        using (ICryptoTransform transform = provider.CreateEncryptor(aes.Key, iv))
                        {
                            byte[] result = transform.TransformFinalBlock(plaintext, 0, plaintext.Length);

                            Assert.AreEqual(cipher, result);
                        }
                    }

                    byte[] decrypted = aes.DecryptBytes(ciphertext);

                    Assert.AreEqual(plaintext, decrypted);

                    // Allow tampering.
                    unchecked {
                        ciphertext[0]++;
                    };

                    aes.DecryptBytes(ciphertext);
                }
            }
        }

        [Test]
        public void Aes256CbcHmacSha256AlgorithmMultiLength()
        {
            using (Aes256CbcHmacSha256Algorithm aes = new Aes256CbcHmacSha256Algorithm())
            {
                for (int i = 1; i < 256; i++)
                {
                    byte[] plaintext = Enumerable.Range(0, i).Select(n => (byte)n).ToArray();
                    byte[] ciphertext = aes.EncryptBytes(plaintext);

                    Assert.AreNotEqual(plaintext, ciphertext);

                    using (AesCryptoServiceProvider provider = new AesCryptoServiceProvider())
                    {
                        byte[] iv = ciphertext.Take(Aes256Cbc.BlockSizeInBytes).ToArray();
                        byte[] cipher = ciphertext.Skip(iv.Length).Take(ciphertext.Length - iv.Length - 32).ToArray();
                        byte[] expectedHash = ciphertext.Skip(ciphertext.Length - 32).ToArray();

                        using (Aes256CbcHmacSha256Key derivedKey = new Aes256CbcHmacSha256Key(aes.Key))
                        {
                            using (ICryptoTransform transform = provider.CreateEncryptor(derivedKey.EncryptionKey, iv))
                            {
                                byte[] result = transform.TransformFinalBlock(plaintext, 0, plaintext.Length);

                                Assert.AreEqual(cipher, result);
                            }

                            using (HMACSHA256 hmac = new HMACSHA256(derivedKey.MACKey))
                            {
                                byte[] actualHash = hmac.ComputeHash(iv.Concat(cipher).ToArray());

                                Assert.AreEqual(expectedHash, actualHash);
                            }
                        }
                    }

                    byte[] decrypted = aes.DecryptBytes(ciphertext);

                    Assert.AreEqual(plaintext, decrypted);

                    // Detect tampering.
                    unchecked {
                        ciphertext[0]++;
                    };

                    Assert.Throws<ArgumentException>(() => aes.DecryptBytes(ciphertext));
                }
            }
        }

        [Test]
        public void Aes256CbcAlgorithmTransform()
        {
            using (Aes256CbcAlgorithm aes = new Aes256CbcAlgorithm()) {
                CheckEncryptDecryptTransforms(aes);
            }
        }

        [Test]
        public void Aes256CbcHmacSha256AlgorithmTransform()
        {
            using (Aes256CbcAlgorithm aes = new Aes256CbcHmacSha256Algorithm()) {
                CheckEncryptDecryptTransforms(aes);
            }
        }

        private static void CheckEncryptDecryptTransforms(SymmetricAlgorithm algorithm)
        {
            for (int i = 1; i < 256; i++)
            {
                byte[] plaintext = Enumerable.Range(0, i).Select(n => (byte)n).ToArray();
                byte[] ciphertext;
                    
                using (MemoryStream encryptedStream = new MemoryStream())
                {
                    using (ICryptoTransform encryptor = algorithm.CreateEncryptor())
                    using (CryptoStream encryptStream = new CryptoStream(encryptedStream, encryptor, CryptoStreamMode.Write)) {
                        encryptStream.Write(plaintext, 0, plaintext.Length);
                    }

                    ciphertext = encryptedStream.ToArray();

                    byte[] decrypted = algorithm.DecryptBytes(ciphertext);

                    Assert.AreEqual(plaintext, decrypted);
                }

                using (MemoryStream decryptedStream = new MemoryStream())
                {
                    using (ICryptoTransform decryptor = algorithm.CreateDecryptor())
                    using (CryptoStream decryptStream = new CryptoStream(decryptedStream, decryptor, CryptoStreamMode.Write)) {
                        decryptStream.Write(ciphertext, 0, ciphertext.Length);
                    }

                    byte[] decrypted = decryptedStream.ToArray();

                    Assert.AreEqual(plaintext, decrypted);
                }
            }
        }
    }
}