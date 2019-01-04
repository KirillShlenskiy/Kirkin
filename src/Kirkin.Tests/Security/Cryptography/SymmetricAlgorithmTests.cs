using System.Linq;

using Kirkin.Security.Cryptography;

using NUnit.Framework;

namespace Kirkin.Tests.Security.Cryptography
{
    public class SymmetricAlgorithmTests
    {
        [Test]
        public void Aes256CbcAlgorithmTests()
        {
            Aes256CbcAlgorithm aes = new Aes256CbcAlgorithm();
            byte[] key = Aes256CbcAlgorithm.GenerateKey();

            for (int i = 1; i < 256; i++)
            {
                byte[] plaintext = Enumerable.Range(0, i).Select(n => (byte)n).ToArray();
                byte[] cyphertext = aes.EncryptBytes(plaintext, key);

                Assert.AreNotEqual(plaintext, cyphertext);

                byte[] decrypted = aes.DecryptBytes(cyphertext, key);

                Assert.AreEqual(plaintext, decrypted);
            }
        }
    }
}