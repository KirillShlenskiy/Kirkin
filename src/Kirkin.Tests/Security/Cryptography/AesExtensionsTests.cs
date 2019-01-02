using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;

using Kirkin.Security.Cryptography;

using NUnit.Framework;

namespace Kirkin.Tests.Security.Cryptography
{
    public class AesExtensionsTests
    {
        [Test]
        public void EncryptDecryptString()
        {
            string expectedText = "Hello!";

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                byte[] encryptedBytes = aes.EncryptString(expectedText);
                string result = aes.DecryptString(encryptedBytes);

                Assert.AreEqual(expectedText, result);
            }
        }

        [Test]
        public void EncryptDecryptStringLong()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 3; i++) {
                sb.Append("Hello there. This is a multi-block string. ");
            }

            string expectedText = sb.ToString();

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                byte[] encryptedBytes = aes.EncryptString(expectedText);
                string result = aes.DecryptString(encryptedBytes);

                Assert.AreEqual(expectedText, result);
            }
        }

        [Test]
        public void EncryptDecryptStringWithHMAC()
        {
            void AppendHmacSuffix(ref byte[] bytes, byte[] key)
            {
                using (HMACSHA256 hmac = new HMACSHA256(key))
                {
                    byte[] tag = hmac.ComputeHash(bytes);
                    byte[] tmp = new byte[bytes.Length + tag.Length];

                    Array.Copy(bytes, 0, tmp, 0, bytes.Length);
                    Array.Copy(tag, 0, tmp, bytes.Length, tag.Length);

                    bytes = tmp;
                }
            }

            void ValidateHmacSuffix(byte[] bytes, byte[] key)
            {
                byte[] messageTag = new byte[32];

                Array.Copy(bytes, bytes.Length - messageTag.Length, messageTag, 0, messageTag.Length);

                using (HMACSHA256 hmac = new HMACSHA256(key))
                {
                    byte[] actualTag = hmac.ComputeHash(bytes, 0, bytes.Length - messageTag.Length);

                    if (!((IStructuralEquatable)actualTag).Equals(messageTag, EqualityComparer<byte>.Default)) {
                        throw new SecurityException("HMAC validation failed.");
                    }
                }
            }

            string expectedText = "Hello!";

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                HmacSha256EncryptionMacKey key = new HmacSha256EncryptionMacKey(aes.Key, "AES256_CBC_HMAC_SHA256");

                Assert.AreNotEqual(aes.Key, key.EncryptionKey);
                Assert.AreNotEqual(aes.Key, key.MACKey);

                aes.Key = key.EncryptionKey;

                byte[] encryptedBytes = aes.EncryptString(expectedText);

                AppendHmacSuffix(ref encryptedBytes, key.MACKey);
                ValidateHmacSuffix(encryptedBytes, key.MACKey);

                string result = aes.DecryptString(encryptedBytes.Take(32).ToArray());

                Assert.AreEqual(expectedText, result);
            }
        }

        [Test]
        public void EncryptDecryptStringPerfAesCryptoServiceProvider()
        {
            string expectedText = "Hello!";

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                for (int i = 0; i < 3000; i++)
                {
                    byte[] encryptedBytes = aes.EncryptString(expectedText);

                    aes.DecryptString(encryptedBytes);
                }
            }
        }

        [Test]
        public void EncryptDecryptStringPerfAesManaged()
        {
            string expectedText = "Hello!";

            using (AesManaged aes = new AesManaged())
            {
                for (int i = 0; i < 3000; i++)
                {
                    byte[] encryptedBytes = aes.EncryptString(expectedText);

                    aes.DecryptString(encryptedBytes);
                }
            }
        }

        [Test]
        public void StreamEncryptDecryptShort()
        {
            string expectedText = "Hello!";
            byte[] expectedBytes = Encoding.UTF8.GetBytes(expectedText);

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                Assert.True(expectedBytes.Length < aes.BlockSize / 8, "Expected text should be shorter than one AES256 block.");

                MemoryStream inputStream = new MemoryStream(expectedBytes);

                using (Stream encryptedStream = aes.EncryptStream(inputStream))
                using (Stream decryptedStream = aes.DecryptStream(encryptedStream))
                {
                    byte[] resultBytes = new byte[expectedBytes.Length];

                    decryptedStream.Read(resultBytes, 0, resultBytes.Length);

                    Assert.AreEqual(expectedBytes, resultBytes);
                }
            }
        }

        [Test]
        public void StreamEncryptDecryptLong()
        {
            string expectedText = "Hello! This is a long long long long string.";
            byte[] expectedBytes = Encoding.UTF8.GetBytes(expectedText);

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                Assert.True(expectedBytes.Length > aes.BlockSize / 8, "Expected text should be longer than one AES256 block.");

                MemoryStream inputStream = new MemoryStream(expectedBytes);

                using (Stream encryptedStream = aes.EncryptStream(inputStream))
                using (Stream decryptedStream = aes.DecryptStream(encryptedStream))
                {
                    byte[] resultBytes = new byte[expectedBytes.Length];

                    decryptedStream.Read(resultBytes, 0, resultBytes.Length);

                    Assert.AreEqual(expectedBytes, resultBytes);
                }
            }
        }

        [Test]
        public void StreamDecryptBufferSmallerThanIVLength()
        {
            string expectedText = "Hello! This is a long long long long string.";
            byte[] expectedBytes = Encoding.UTF8.GetBytes(expectedText);

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                Assert.True(expectedBytes.Length > aes.BlockSize / 8, "Expected text should be longer than one AES256 block.");

                MemoryStream inputStream = new MemoryStream(expectedBytes);

                using (Stream encryptedStream = aes.EncryptStream(inputStream))
                using (Stream decryptedStream = aes.DecryptStream(encryptedStream))
                {
                    byte[] resultBytes = new byte[expectedBytes.Length];

                    for (int i = 0; i < resultBytes.Length; i++) {
                        decryptedStream.Read(resultBytes, i, 1);
                    }

                    Assert.AreEqual(expectedBytes, resultBytes);
                }
            }
        }

        [Test]
        [Ignore("Requires valid file.")]
        public void EncryptLargeFile()
        {
            string filePath = @"<path>";

            using (FileStream fs = File.OpenRead(filePath))
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            using (Stream encryptedStream = aes.EncryptStream(fs)) {
                ConsumeStream(encryptedStream);
            }
        }

        [Test]
        [Ignore("Requires valid file.")]
        public void EncryptDecryptLargeFile()
        {
            string filePath = @"<path>";

            using (FileStream fs = File.OpenRead(filePath))
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            using (Stream encryptedStream = aes.EncryptStream(fs))
            using (Stream decryptedStream = aes.DecryptStream(encryptedStream)) {
                ConsumeStream(decryptedStream);
            }
        }

        private static void ConsumeStream(Stream stream)
        {
            byte[] buffer = new byte[81920];
            
            while (true)
            {
                if (stream.Read(buffer, 0, buffer.Length) == 0) {
                    break;
                }
            }
        }
    }
}