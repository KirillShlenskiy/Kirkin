using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Kirkin.Cryptography
{
    /// <summary>
    /// AesManaged wrapper with a misuse-resistant API.
    /// </summary>
    /// <remarks>
    /// Originally based on Encryptamajig (MIT license - https://github.com/jbubriski/Encryptamajig).
    /// </remarks>
    public sealed class AES256Encryption
    {
        private static readonly Encoding SafeUTF8
            = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        private const int BlockBitSize = 128;
        private const int KeyBitSize = 256; // AES 256.
        private const int SaltBitSize = 128;
        private const int Iterations = 10000;

        /// <summary>
        /// Character encoding used by this instance.
        /// </summary>
        public Encoding Encoding { get; }

        /// <summary>
        /// Creates a new instance of <see cref="AES256Encryption"/> with the default character encoding (UTF-8 no BOM).
        /// </summary>
        public AES256Encryption()
            : this(SafeUTF8)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="AES256Encryption"/> with the given character encoding.
        /// </summary>
        public AES256Encryption(Encoding encoding)
        {
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            Encoding = encoding;
        }

        /// <summary>
        /// Encrypts the plain text input using the given secret.
        /// </summary>
        public string Encrypt(string plainText, string secret)
        {
            if (string.IsNullOrEmpty(plainText)) throw new ArgumentException(nameof(plainText));
            if (string.IsNullOrEmpty(secret)) throw new ArgumentException(nameof(secret));

            using (Rfc2898DeriveBytes keyDerivationFunction = new Rfc2898DeriveBytes(secret, SaltBitSize / 8, Iterations))
            {
                byte[] saltBytes = keyDerivationFunction.Salt;
                byte[] keyBytes = keyDerivationFunction.GetBytes(KeyBitSize / 8);

                // The default Cipher Mode is CBC and the Padding is PKCS7 which are both good.
                using (AesManaged aes = new AesManaged { KeySize = KeyBitSize, BlockSize = BlockBitSize })
                {
                    byte[] ivBytes = aes.IV;

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (ICryptoTransform encryptor = aes.CreateEncryptor(keyBytes, ivBytes))
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream, Encoding)) {
                            streamWriter.Write(plainText);
                        }

                        byte[] encryptedTextBytes = memoryStream.ToArray();

                        // Result format: 128 bits of salt, 128 bits of IV, 128 bits of encrypted text.
                        byte[] result = new byte[saltBytes.Length + ivBytes.Length + encryptedTextBytes.Length];

                        Array.Copy(saltBytes, 0, result, 0, saltBytes.Length);
                        Array.Copy(ivBytes, 0, result, saltBytes.Length, ivBytes.Length);
                        Array.Copy(encryptedTextBytes, 0, result, saltBytes.Length + ivBytes.Length, encryptedTextBytes.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        /// <summary>
        /// Decrypts the encrypted text using the provided secret.
        /// </summary>
        public string Decrypt(string encryptedText, string secret)
        {
            if (string.IsNullOrEmpty(encryptedText)) throw new ArgumentException(nameof(encryptedText));
            if (string.IsNullOrEmpty(secret)) throw new ArgumentException(nameof(secret));

            byte[] allBytes = Convert.FromBase64String(encryptedText);

            // Result format: 128 bits of salt, 128 bits of IV, 128 bits of encrypted text.
            byte[] saltBytes = new byte[SaltBitSize / 8];
            byte[] ivBytes = new byte[BlockBitSize / 8];
            byte[] encryptedTextBytes = new byte[allBytes.Length - saltBytes.Length - ivBytes.Length];

            Array.Copy(allBytes, 0, saltBytes, 0, saltBytes.Length);
            Array.Copy(allBytes, saltBytes.Length, ivBytes, 0, ivBytes.Length);
            Array.Copy(allBytes, saltBytes.Length + ivBytes.Length, encryptedTextBytes, 0, encryptedTextBytes.Length);

            using (Rfc2898DeriveBytes keyDerivationFunction = new Rfc2898DeriveBytes(secret, saltBytes, Iterations))
            {
                byte[] keyBytes = keyDerivationFunction.GetBytes(KeyBitSize / 8);

                // The default Cipher Mode is CBC and the Padding is PKCS7 which are both good.
                using (AesManaged aes = new AesManaged { KeySize = KeyBitSize, BlockSize = BlockBitSize })
                using (ICryptoTransform decryptor = aes.CreateDecryptor(keyBytes, ivBytes))
                using (MemoryStream memoryStream = new MemoryStream(encryptedTextBytes))
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                using (StreamReader streamReader = new StreamReader(cryptoStream, Encoding)) {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}