using System.IO;
using System.Security.Cryptography;
using System.Text;

using Kirkin.Security.Cryptography;

using NUnit.Framework;

namespace Kirkin.Tests.Security.Cryptography
{
    public class AesExtensionsTests
    {
        [Test]
        public void StreamEncryptDecrypt()
        {
            string expectedText = "Hello! This is a long long long long string.";
            byte[] expectedBytes = Encoding.UTF8.GetBytes(expectedText);

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                MemoryStream inputStream = new MemoryStream(expectedBytes);

                using (Stream encryptedStream = aes.EncryptStream(inputStream))
                using (Stream decryptedStream = aes.DecryptStream(encryptedStream))
                {
                    byte[] resultBytes = new byte[decryptedStream.Length];

                    decryptedStream.Read(resultBytes, 0, resultBytes.Length);

                    Assert.AreEqual(expectedBytes, resultBytes);
                }
            }
        }
    }
}