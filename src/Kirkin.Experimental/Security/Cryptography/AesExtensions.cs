using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Kirkin.Security.Cryptography
{
    public static class AesExtensions
    {
        private static readonly Encoding SafeUTF8
            = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        public static byte[] DecryptBytes(this Aes aes, byte[] encryptedBytes, byte[] key)
        {
            using (MemoryStream inputStream = new MemoryStream(encryptedBytes))
            using (Stream decryptedStream = aes.DecryptStream(inputStream, key))
            using (MemoryStream outputStream = new MemoryStream((int)inputStream.Length))
            {
                decryptedStream.CopyTo(outputStream);

                return outputStream.ToArray();
            }
        }

        public static Stream DecryptStream(this Aes aes, Stream encryptedStream, byte[] key, bool disposeAes = false)
        {
            byte[] iv = new byte[aes.BlockSize / 8];

            if (encryptedStream.Read(iv, 0, iv.Length) != iv.Length) {
                throw new InvalidOperationException($"{aes.BlockSize}-bit IV excepted.");
            }

            ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);

            return new AesDecryptStream(encryptedStream, aes, decryptor, disposeAes);
        }

        sealed class AesDecryptStream : CryptoStream
        {
            public Aes Aes { get; }
            public ICryptoTransform Transform { get; }
            public bool DisposeAes { get; }

            public AesDecryptStream(Stream stream, Aes aes, ICryptoTransform transform, bool disposeAes)
                : base(stream, transform, CryptoStreamMode.Read)
            {
                if (aes == null) throw new ArgumentNullException(nameof(aes));
                if (transform == null) throw new ArgumentNullException(nameof(transform));

                Aes = aes;
                Transform = transform;
                DisposeAes = disposeAes;
            }

            protected override void Dispose(bool disposing)
            {
                Transform.Dispose();

                if (DisposeAes) {
                    Aes.Dispose();
                }

                base.Dispose(disposing);
            }
        }

        public static byte[] EncryptBytes(this Aes aes, byte[] bytes)
        {
            using (MemoryStream inputStream = new MemoryStream(bytes))
            using (Stream encryptedStream = aes.EncryptStream(inputStream))
            using (MemoryStream outputStream = new MemoryStream((int)encryptedStream.Length))
            {
                encryptedStream.CopyTo(outputStream);

                return outputStream.ToArray();
            }
        }

        public static Stream EncryptStream(this Aes aes, Stream inputStream)
        {
            MemoryStream outputStream = new MemoryStream();

            aes.EncryptStream(inputStream, outputStream);

            outputStream.Position = 0;

            return outputStream;
        }

        public static void EncryptStream(this Aes aes, Stream inputStream, Stream outputStream)
        {
            byte[] iv = aes.IV;

            if (iv.Length != 16) {
                throw new InvalidOperationException("Expecting IV to be 16 bytes exactly.");
            }

            outputStream.Write(iv, 0, iv.Length);

            using (ICryptoTransform encryptor = aes.CreateEncryptor())
            using (CryptoStream encryptStream = new CryptoStream(inputStream, encryptor, CryptoStreamMode.Read)) {
                encryptStream.CopyTo(outputStream);
            }
        }
    }
}