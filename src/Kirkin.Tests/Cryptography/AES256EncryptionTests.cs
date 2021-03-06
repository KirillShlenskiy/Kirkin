﻿using System;

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
        public void DecryptOnlyPerf()
        {
            string encrypted = "gAAAABAnAAAvtvNVwx0+pAefy+LhcUjcjA/GbLzUxsWmhdsRqZVn/2AbugBfon6z0h4sXTXSXqJmUnWV+a75UFERmoArruyfVeXtwe6TrHlWT8sAqLWjug==";
            AES256Encryption aes = new AES256Encryption();

            Assert.AreEqual("The quick brown fox jumps over the lazy dog", aes.DecryptBase64(encrypted, "Secret"));
        }

        [Test]
        public void MinResultLength72Bytes()
        {
            string text = "a";
            string secret = "a";
            AES256Encryption aes = new AES256Encryption();
            string encrypted = aes.EncryptBase64(text, secret);
            byte[] bytes = Convert.FromBase64String(encrypted);

            Assert.AreEqual(72, bytes.Length);
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
            string text = "The quick brown fox jumps over the lazy dog";
            string secret = "Secret";

            foreach (int iterationCount in new[] { 1, 10, 100, 1000, 10000, 20000, 50000 })
            {
                AES256Encryption aes = new AES256Encryption(iterationCount);
                byte[] encrypted = aes.Encrypt(text, secret);

                Assert.AreEqual(text, aes.Decrypt(encrypted, secret));
            }
        }
    }
}