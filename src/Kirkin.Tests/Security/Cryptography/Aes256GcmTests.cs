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
        public void BasicSerialEncryption()
        {
            byte[] plaintextBytes = CryptoRandom.GetRandomBytes(1024);
            byte[] key = CryptoRandom.GetRandomBytes(32);
            byte[] nonce = CryptoRandom.GetRandomBytes(12);
            byte[] decryptedBytes = EncryptDecrypt(plaintextBytes, key, nonce);

            Assert.AreEqual(plaintextBytes, decryptedBytes);
        }

        [Test]
        public void Perf128BitKeySerial()
        {
            byte[] plaintextBytes = CryptoRandom.GetRandomBytes(1024 * 1024 * 8);
            byte[] key = CryptoRandom.GetRandomBytes(16);
            byte[] nonce = CryptoRandom.GetRandomBytes(12);

            for (int i = 0; i < 10; i++) {
                EncryptDecrypt(plaintextBytes, key, nonce);
            }
        }

        [Test]
        public void Perf256BitKeySerial()
        {
            byte[] plaintextBytes = CryptoRandom.GetRandomBytes(1024 * 1024 * 8);
            byte[] key = CryptoRandom.GetRandomBytes(32);
            byte[] nonce = CryptoRandom.GetRandomBytes(12);

            for (int i = 0; i < 10; i++) {
                EncryptDecrypt(plaintextBytes, key, nonce);
            }
        }

        private byte[] EncryptDecrypt(byte[] plaintextBytes, byte[] key, byte[] nonce)
        {
            // Shared BouncyCastle params.
            KeyParameter keyParameter = new KeyParameter(key);
            AeadParameters aeadParameters = new AeadParameters(keyParameter, MAC_LENGTH_BYTES * 8, nonce);

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

            return decryptedBytes;
        }
    }
}