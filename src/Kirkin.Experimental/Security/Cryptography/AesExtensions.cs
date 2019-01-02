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

        public static byte[] DecryptBytes(this Aes aes, byte[] encryptedBytes)
        {
            using (MemoryStream inputStream = new MemoryStream(encryptedBytes))
            using (Stream decryptedStream = aes.DecryptStream(inputStream))
            using (MemoryStream outputStream = new MemoryStream((int)inputStream.Length))
            {
                decryptedStream.CopyTo(outputStream);

                return outputStream.ToArray();
            }
        }

        public static Stream DecryptStream(this Aes aes, Stream encryptedStream, bool disposeAesWhenClosed = false)
        {
            byte[] iv = new byte[aes.BlockSize / 8];

            if (encryptedStream.Read(iv, 0, iv.Length) != iv.Length) {
                throw new InvalidOperationException($"{aes.BlockSize}-bit IV excepted.");
            }

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, iv);

            return new AesDecryptStream(encryptedStream, aes, decryptor, disposeAesWhenClosed);
        }

        public static string DecryptString(this Aes aes, byte[] encryptedText)
        {
            return SafeUTF8.GetString(aes.DecryptBytes(encryptedText));
        }

        sealed class AesDecryptStream : CryptoStream
        {
            public Aes Aes { get; }
            public ICryptoTransform Transform { get; }
            public bool DisposeAesWhenClosed { get; }

            public AesDecryptStream(Stream stream, Aes aes, ICryptoTransform transform, bool disposeAesWhenClosed)
                : base(stream, transform, CryptoStreamMode.Read)
            {
                if (aes == null) throw new ArgumentNullException(nameof(aes));
                if (transform == null) throw new ArgumentNullException(nameof(transform));

                Aes = aes;
                Transform = transform;
                DisposeAesWhenClosed = disposeAesWhenClosed;
            }

            protected override void Dispose(bool disposing)
            {
                Transform.Dispose();

                if (DisposeAesWhenClosed) {
                    Aes.Dispose();
                }

                base.Dispose(disposing);
            }
        }

        // Result format: iv + cipher.
        public static byte[] EncryptBytes(this Aes aes, byte[] bytes)
        {
            using (ICryptoTransform transform = aes.CreateEncryptor())
            {
                if (!transform.CanTransformMultipleBlocks) {
                    throw new NotSupportedException("AES encryptor does not support multi-block transforms.");
                }

                int blockSizeInBytes = aes.BlockSize / 8;
                int blockCount = bytes.Length / blockSizeInBytes + 1;
                byte[] iv = aes.IV;
                byte[] result = new byte[iv.Length + blockCount * blockSizeInBytes];
                int count = 0;
                int resultOffset = iv.Length;

                if (blockCount > 1)
                {
                    count = (blockCount - 1) * blockSizeInBytes;
                    resultOffset += transform.TransformBlock(bytes, 0, count, result, resultOffset);
                }

                byte[] finalBlock = transform.TransformFinalBlock(bytes, count, bytes.Length - count);

                Array.Copy(finalBlock, 0, result, resultOffset, finalBlock.Length);
                Array.Copy(iv, 0, result, 0, iv.Length);

                return result;
            }
        }

        public static Stream EncryptStream(this Aes aes, Stream inputStream, bool disposeAesWhenClosed = false)
        {
            return new AesEncryptStream(inputStream, aes, disposeAesWhenClosed);
        }

        public static byte[] EncryptString(this Aes aes, string plainText)
        {
            return aes.EncryptBytes(SafeUTF8.GetBytes(plainText));
        }

        sealed class AesEncryptStream : Stream
        {
            public Aes Aes { get; }
            public bool DisposeAesWhenClosed { get; }

            private ICryptoTransform _encryptor;

            public ICryptoTransform Encryptor
            {
                get
                {
                    if (_encryptor == null) {
                        _encryptor = Aes.CreateEncryptor();
                    }

                    return _encryptor;
                }
            }

            private long _position;
            private Stream InputStream;
            private CryptoStream _cryptoStream;

            private CryptoStream CryptoStream
            {
                get
                {
                    if (_cryptoStream == null) {
                        _cryptoStream = new CryptoStream(InputStream, Encryptor, CryptoStreamMode.Read);
                    }

                    return _cryptoStream;
                }
            }

            public override bool CanRead
            {
                get
                {
                    return CryptoStream.CanRead;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return CryptoStream.CanSeek;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return false;
                }
            }

            public override long Length
            {
                get
                {
                    // Actual data length is: stream length + IV length + padding.
                    return CryptoStream.Length + Aes.BlockSize / 8;
                }
            }

            public override long Position
            {
                get
                {
                    return _position;
                }
                set
                {
                    if (!CanSeek) throw new NotSupportedException();
                    if (value < 0 || value > Length) throw new ArgumentOutOfRangeException();

                    _position = value;
                }
            }

            public AesEncryptStream(Stream stream, Aes aes, bool disposeAesWhenClosed)
            {
                InputStream = stream ?? throw new ArgumentNullException(nameof(stream));
                Aes = aes ?? throw new ArgumentNullException(nameof(aes));
                DisposeAesWhenClosed = disposeAesWhenClosed;
            }

            public override void Flush()
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int totalRead = 0;

                if (_position < Aes.BlockSize / 8)
                {
                    // Read IV.
                    int ivCount = Aes.BlockSize / 8 - (int)_position;

                    Array.Copy(Aes.IV, _position, buffer, offset, ivCount);

                    totalRead += ivCount;
                }

                if (totalRead < count) {
                    totalRead += CryptoStream.Read(buffer, offset + totalRead, count - totalRead);
                }

                _position += totalRead;

                return totalRead;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                if (origin == SeekOrigin.Begin)
                {
                    Position = offset;
                }
                else if (origin == SeekOrigin.Current)
                {
                    Position += offset;
                }
                else
                {
                    throw new NotSupportedException();
                }

                return Position;
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override void Close()
            {
                base.Close();

                _cryptoStream?.Close();
                _encryptor?.Dispose();

                InputStream.Close();

                if (DisposeAesWhenClosed) {
                    Aes.Dispose();
                }
            }
        }
    }
}