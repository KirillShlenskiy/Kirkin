using System.IO;
using System.Linq;
using System.Security.Cryptography;

using Kirkin.Security.Cryptography;
using Kirkin.Security.Cryptography.Internal;

using NUnit.Framework;

namespace Kirkin.Tests.Security.Cryptography
{
    public class SymmetricTransformTests
    {
        [Test]
        public void Aes256CbcTransformSmall()
        {
            using (Aes256Cbc aes = new Aes256Cbc()) {
                CheckEncryptDecryptTransformsSmall(aes);
            }
        }

        [Test]
        public void Aes256CbcHmacSha256TransformSmall()
        {
            using (Aes256Cbc aes = new Aes256CbcHmacSha256()) {
                CheckEncryptDecryptTransformsSmall(aes);
            }
        }

        private static void CheckEncryptDecryptTransformsSmall(SymmetricCryptoFormatter formatter)
        {
            for (int i = 1; i < 256; i++)
            {
                byte[] plaintext = Enumerable.Range(0, i).Select(n => (byte)n).ToArray();
                byte[] ciphertext;
                    
                using (MemoryStream encryptedStream = new MemoryStream())
                {
                    using (ICryptoTransform encryptor = new SymmetricEncryptTransform(formatter))
                    using (CryptoStream encryptStream = new CryptoStream(encryptedStream, encryptor, CryptoStreamMode.Write)) {
                        encryptStream.Write(plaintext, 0, plaintext.Length);
                    }

                    ciphertext = encryptedStream.ToArray();

                    byte[] decrypted = formatter.DecryptBytes(ciphertext);

                    Assert.AreEqual(plaintext, decrypted);
                }

                using (MemoryStream decryptedStream = new MemoryStream())
                {
                    using (ICryptoTransform decryptor = new SymmetricDecryptTransform(formatter))
                    using (CryptoStream decryptStream = new CryptoStream(decryptedStream, decryptor, CryptoStreamMode.Write)) {
                        decryptStream.Write(ciphertext, 0, ciphertext.Length);
                    }

                    byte[] decrypted = decryptedStream.ToArray();

                    Assert.AreEqual(plaintext, decrypted);
                }
            }
        }

        [Test]
        public void Aes256CbcTransformLarge()
        {
            using (Aes256Cbc aes = new Aes256Cbc()) {
                CheckEncryptDecryptTransformsLarge(aes);
            }
        }

        [Test]
        public void Aes256CbcHmacSha256TransformLarge()
        {
            using (Aes256Cbc aes = new Aes256CbcHmacSha256()) {
                CheckEncryptDecryptTransformsLarge(aes);
            }
        }

        private static void CheckEncryptDecryptTransformsLarge(SymmetricCryptoFormatter formatter)
        {
            // Work with messages that either fit into single chunk, or not.
            foreach (double chunkFillRatio in new[] { 0.9, 1.0, 1.1, 2.1 })
            {
                byte[] plaintext;
                byte[] ciphertext;

                using (MemoryStream encryptedStream = new MemoryStream())
                {
                    using (ICryptoTransform encryptor = new SymmetricEncryptTransform(formatter))
                    {
                        int plaintextLength = (int)(ChunkedTransform.DefaultChunkSize * chunkFillRatio);

                        plaintext = Enumerable.Range(0, plaintextLength).Select(n => (byte)n).ToArray();

                        using (CryptoStream encryptStream = new CryptoStream(encryptedStream, encryptor, CryptoStreamMode.Write)) {
                            encryptStream.Write(plaintext, 0, plaintext.Length);
                        }
                    }

                    ciphertext = encryptedStream.ToArray();
                }

                using (MemoryStream decryptedStream = new MemoryStream(ciphertext.Length))
                {
                    using (ICryptoTransform decryptor = new SymmetricDecryptTransform(formatter))
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