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
            using (Aes256CbcAlgorithm aes = new Aes256CbcAlgorithm())
            {
                for (int i = 1; i < 256; i++)
                {
                    byte[] plaintext = Enumerable.Range(0, i).Select(n => (byte)n).ToArray();
                    byte[] cyphertext = aes.EncryptBytes(plaintext);

                    Assert.AreNotEqual(plaintext, cyphertext);

                    byte[] decrypted = aes.DecryptBytes(cyphertext);

                    Assert.AreEqual(plaintext, decrypted);
                }
            }
        }
    }
}