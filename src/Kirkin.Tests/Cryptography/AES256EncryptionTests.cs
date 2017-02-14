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
        public void MinResultLength48Bytes()
        {
            string text = "a";
            string secret = "a";
            AES256Encryption aes = new AES256Encryption();
            string encrypted = aes.EncryptBase64(text, secret);
            byte[] bytes = Convert.FromBase64String(encrypted);

            Assert.AreEqual(48, bytes.Length);
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
    }
}