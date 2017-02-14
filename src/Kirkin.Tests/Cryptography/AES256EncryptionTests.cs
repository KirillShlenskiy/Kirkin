using Kirkin.Cryptography;

using NUnit.Framework;

namespace Kirkin.Tests.Cryptography
{
    public class AES256EncryptionTests
    {
        [Test]
        public void RoundtripTest()
        {
            string text = "The quick brown fox jumps over the lazy dog";
            string secret = "Secret";
            AES256Encryption encryption = new AES256Encryption();
            string encrypted = encryption.Encrypt(text, secret);

            Assert.AreNotEqual(text, encrypted);

            string decrypted = encryption.Decrypt(encrypted, secret);

            Assert.AreEqual(text, decrypted); // Roundtrip.
            Assert.AreNotEqual(encrypted, encryption.Encrypt(text, secret)); // Randomness.
        }
    }
}