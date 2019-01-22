using System;
using System.Security.Cryptography;
using System.Text;

using NUnit.Framework;

namespace Kirkin.Tests.Experimental
{
    public class RsaTests
    {
        [Test]
        public void BasicRsa()
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
            {
                string plaintext = "zzz";
                byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                byte[] encryptedBytes;

                Console.WriteLine("RSA blob:");

                byte[] publicKey = rsa.ExportCspBlob(false);

                Console.WriteLine(Convert.ToBase64String(publicKey));
                Console.WriteLine("With private parameters:");

                using (RSACryptoServiceProvider rsaEncryptor = new RSACryptoServiceProvider())
                {
                    rsaEncryptor.ImportCspBlob(publicKey);

                    encryptedBytes = rsaEncryptor.Encrypt(plaintextBytes, true);
                }

                byte[] privateKey = rsa.ExportCspBlob(true);

                Console.WriteLine(Encoding.UTF8.GetString(privateKey));
                Console.WriteLine("Encrypted:");
                Console.WriteLine(Convert.ToBase64String(encryptedBytes));

                using (RSACryptoServiceProvider rsaDecryptor = new RSACryptoServiceProvider())
                {
                    rsaDecryptor.ImportCspBlob(privateKey);

                    byte[] decryptedBytes = rsaDecryptor.Decrypt(encryptedBytes, true);

                    Console.WriteLine(Encoding.UTF8.GetString(decryptedBytes));
                }
            }
        }
    }
}