using System;

using Kirkin.Cryptography;

using NUnit.Framework;

namespace Kirkin.Tests.Cryptography
{
    public class AES256EncryptionTests
    {
        [Test]
        public void RoundtripSucceeds()
        {
            string text = "The quick brown fox jumps over the lazy dog";
            string secret = "Secret";
            AES256Encryption aes = new AES256Encryption();

            string encrypted = aes.EncryptBase64(text, secret);

            Assert.AreNotEqual(text, encrypted);

            string decrypted = aes.DecryptBase64(encrypted, secret);

            Assert.AreEqual(text, decrypted); // Roundtrip.
        }

        [Test]
        public void Randomness()
        {
            string text = "The quick brown fox jumps over the lazy dog";
            string secret = "Secret";
            AES256Encryption aes = new AES256Encryption();
            string encrypted1 = aes.EncryptBase64(text, secret);
            string encrypted2 = aes.EncryptBase64(text, secret);

            Assert.AreNotEqual(encrypted1, encrypted2); // Randomness.
        }

        [Test]
        public void MinResultLength52Bytes()
        {
            string text = "a";
            string secret = "a";
            AES256Encryption aes = new AES256Encryption();
            string encrypted = aes.EncryptBase64(text, secret);
            byte[] bytes = Convert.FromBase64String(encrypted);

            Assert.AreEqual(52, bytes.Length);
        }

        [Test]
        public void VeryLongStringEncryption()
        {
            string text = "The quick brown fox jumps over the lazy dog";
            string secret = "Secret";

            while (text.Length < 8000) text += text;
            while (secret.Length < 8000) secret += secret;

            AES256Encryption aes = new AES256Encryption();
            string encrypted = aes.EncryptBase64(text, secret);

            Assert.AreNotEqual(text, encrypted);

            string decrypted = aes.DecryptBase64(encrypted, secret);

            Assert.AreEqual(text, decrypted); // Roundtrip.
        }

        [Test]
        public void TrickyCharacters()
        {
            AES256Encryption aes = new AES256Encryption();
            string encrypted = aes.EncryptBase64("ヾ(｀⌒´メ)ノ″😋ъ", "zzz");

            Assert.AreEqual("ヾ(｀⌒´メ)ノ″😋ъ", aes.DecryptBase64(encrypted, "zzz"));
        }

        [Test]
        public void VariableIterationCount()
        {
            AES256Encryption aes = new AES256Encryption();
            string text = "The quick brown fox jumps over the lazy dog";
            string secret = "Secret";

            foreach (int iterationCount in new[] { 1, 10, 100, 1000, 10000, 20000, 50000 })
            {
                byte[] encrypted = aes.Encrypt(text, secret, iterationCount);

                Assert.AreEqual(text, aes.Decrypt(encrypted, secret));
            }
        }

        [Test]
        public void ReadWriteInt32()
        {
            int value = 123;
            byte[] bytes = new byte[4];

            Bits.WriteInt32(bytes, 0, value);

            Assert.AreEqual((byte)value, bytes[3]); // Check most significant byte first.

            int roundtrip = Bits.ReadInt32(bytes, 0);

            Assert.AreEqual(value, roundtrip);
        }

        static class Bits
        {
            static Bits()
            {
                if (!BitConverter.IsLittleEndian) {
                    throw new NotSupportedException("Big endian architecture not supported.");
                }
            }

            /// <summary>
            /// Reads 4 bytes at the given offset as an Int32 (most significant byte first).
            /// </summary>
            internal static int ReadInt32(byte[] bytes, int startIndex)
            {
                return
                    bytes[startIndex] << 24 |
                    bytes[startIndex + 1] << 16 |
                    bytes[startIndex + 2] << 8 |
                    bytes[startIndex + 3];
            }

            /// <summary>
            /// Writes the given Int32 value as 4 bytes at the given offset (most significant byte first).
            /// </summary>
            internal static void WriteInt32(byte[] bytes, int startIndex, int value)
            {
                bytes[startIndex] = (byte)(value >> 24);
                bytes[startIndex + 1] = (byte)(value >> 16);
                bytes[startIndex + 2] = (byte)(value >> 8);
                bytes[startIndex + 3] = (byte)value;
            }
        }
    }
}