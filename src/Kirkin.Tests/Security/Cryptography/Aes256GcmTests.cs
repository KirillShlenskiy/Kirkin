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
            byte[] plaintextBytes = CryptoRandom.GetRandomBytes(4096);
            byte[] key = CryptoRandom.GetRandomBytes(32);
            byte[] iv = CryptoRandom.GetRandomBytes(12);

            // Shared BouncyCastle params.
            KeyParameter keyParameter = new KeyParameter(key);
            AeadParameters aeadParameters = new AeadParameters(keyParameter, MAC_LENGTH_BYTES * 8, iv);

            // Encrypt.
            GcmBlockCipher encryptor = new GcmBlockCipher(new AesEngine());

            encryptor.Init(true, aeadParameters);

            byte[] ciphertextBytes = new byte[encryptor.GetOutputSize(plaintextBytes.Length)];

            {
                int offset = encryptor.ProcessBytes(plaintextBytes, 0, plaintextBytes.Length, ciphertextBytes, 0);
                int finalLength = offset + encryptor.DoFinal(ciphertextBytes, offset);

                if (finalLength != ciphertextBytes.Length) {
                    Array.Resize(ref ciphertextBytes, finalLength);
                }
            }

            // Decrypt.
            GcmBlockCipher decryptor = new GcmBlockCipher(new AesEngine());

            decryptor.Init(false, aeadParameters);

            byte[] roundtripBytes = new byte[decryptor.GetOutputSize(ciphertextBytes.Length)];

            {
                int offset = decryptor.ProcessBytes(ciphertextBytes, 0, ciphertextBytes.Length, roundtripBytes, 0);
                int finalLength = offset + decryptor.DoFinal(roundtripBytes, offset);

                if (finalLength != roundtripBytes.Length) {
                    Array.Resize(ref roundtripBytes, finalLength);
                }
            }

            Assert.AreEqual(plaintextBytes, roundtripBytes);
        }
    }
}
