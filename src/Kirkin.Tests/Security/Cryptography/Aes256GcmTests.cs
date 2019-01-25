using System;

using Kirkin.Security.Cryptography.Internal;

using NUnit.Framework;

using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace Kirkin.Tests.Security.Cryptography
{
    public class Aes256GcmTests
    {
        const int MAC_LENGTH_BYTES = 16;

        [Test]
        public void BasicEncryption()
        {
            byte[] plaintextBytes = CryptoRandom.GetRandomBytes(1024 * 8);
            byte[] key = CryptoRandom.GetRandomBytes(32);
            byte[] iv = CryptoRandom.GetRandomBytes(12);

            // Shared BouncyCastle params.
            KeyParameter keyParameter = new KeyParameter(key);
            AeadParameters aeadParameters = new AeadParameters(keyParameter, MAC_LENGTH_BYTES * 8, iv);

            // Encrypt.
            byte[] encryptedBytes;

            {
                GcmBlockCipher encryptor = new GcmBlockCipher(new AesEngine());

                encryptor.Init(true, aeadParameters);

                encryptedBytes = new byte[encryptor.GetOutputSize(plaintextBytes.Length)];

                int offset = encryptor.ProcessBytes(plaintextBytes, 0, plaintextBytes.Length, encryptedBytes, 0);
                int finalLength = offset + encryptor.DoFinal(encryptedBytes, offset);

                if (finalLength != encryptedBytes.Length) {
                    Array.Resize(ref encryptedBytes, finalLength);
                }
            }

            // Decrypt.
            byte[] decryptedBytes;

            {
                GcmBlockCipher decryptor = new GcmBlockCipher(new AesEngine());

                decryptor.Init(false, aeadParameters);

                decryptedBytes = new byte[decryptor.GetOutputSize(encryptedBytes.Length)];

                int offset = decryptor.ProcessBytes(encryptedBytes, 0, encryptedBytes.Length, decryptedBytes, 0);
                int finalLength = offset + decryptor.DoFinal(decryptedBytes, offset);

                if (finalLength != decryptedBytes.Length) {
                    Array.Resize(ref decryptedBytes, finalLength);
                }
            }

            Assert.AreEqual(plaintextBytes, decryptedBytes);
        }
    }
}
