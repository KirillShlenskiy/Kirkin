using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Kirkin.Cryptography
{
    /// <summary>
    /// Simple AesManaged wrapper.
    /// </summary>
    /// <remarks>
    /// Originally based on Encryptamajig (MIT license - https://github.com/jbubriski/Encryptamajig).
    /// </remarks>
    public sealed class AES256Encryption : ICryptoKernel
    {
        private static readonly Encoding SafeUTF8
            = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        private const int BlockBitSize = 128;
        private const int KeyBitSize = 256; // AES 256.
        private const int SaltBitSize = 128;

        /// <summary>
        /// Character encoding used by this instance.
        /// </summary>
        internal Encoding Encoding { get; }

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
        internal AES256Encryption(Encoding encoding)
        {
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            Encoding = encoding;
        }

        /// <summary>
        /// Encrypts the plain text input using the given secret.
        /// </summary>
        /// <param name="plainText">Plain text to be encrypted.</param>
        /// <param name="secret">Secret passphrase used to encrypt plain text.</param>
        /// <param name="iterations">
        /// Number of PBKDF2-HMAC-SHA1 iterations used when hashing the secret.
        /// The higher this number, the stronger the encryption.
        /// </param>
        public byte[] Encrypt(string plainText, string secret, int iterations = 10000)
        {
            if (plainText == null) throw new ArgumentNullException(nameof(plainText));
            if (secret == null) throw new ArgumentNullException(nameof(secret));

            // Rfc2898DeriveBytes always uses UTF8 no BOM.
            using (Rfc2898DeriveBytes keyDerivationFunction = new Rfc2898DeriveBytes(secret, SaltBitSize / 8, iterations))
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

                        // Result format: 32 bits of SHA1 iteration count, 128 bits of salt,
                        // 128 bits of IV, 128 (or more) bits of encrypted text.
                        byte[] result = new byte[sizeof(int) + saltBytes.Length + ivBytes.Length + encryptedTextBytes.Length];
                        byte[] iterationCountBytes = BitConverter.GetBytes(iterations);

                        Debug.Assert(iterationCountBytes.Length == 4, "Iteration bytes expected to be a 32-bit value.");

                        Array.Copy(iterationCountBytes, 0, result, 0, iterationCountBytes.Length);
                        Array.Copy(saltBytes, 0, result, iterationCountBytes.Length, saltBytes.Length);
                        Array.Copy(ivBytes, 0, result, iterationCountBytes.Length + saltBytes.Length, ivBytes.Length);
                        Array.Copy(encryptedTextBytes, 0, result, iterationCountBytes.Length + saltBytes.Length + ivBytes.Length, encryptedTextBytes.Length);

                        return result;
                    }
                }
            }
        }

        /// <summary>
        /// Decrypts the byte array using the provided secret.
        /// </summary>
        public string Decrypt(byte[] encryptedBytes, string secret)
        {
            if (encryptedBytes == null) throw new ArgumentNullException(nameof(encryptedBytes));
            if (secret == null) throw new ArgumentNullException(nameof(secret));

            // Expected format: 32 bits of SHA1 iteration count, 128 bits of salt,
            // 128 bits of IV, 128 (or more) bits of encrypted text.
            byte[] iterationCountBytes = new byte[sizeof(int)];
            byte[] saltBytes = new byte[SaltBitSize / 8];
            byte[] ivBytes = new byte[BlockBitSize / 8];
            byte[] encryptedTextBytes = new byte[encryptedBytes.Length - iterationCountBytes.Length - saltBytes.Length - ivBytes.Length];

            Array.Copy(encryptedBytes, 0, iterationCountBytes, 0, iterationCountBytes.Length);
            Array.Copy(encryptedBytes, iterationCountBytes.Length, saltBytes, 0, saltBytes.Length);
            Array.Copy(encryptedBytes, iterationCountBytes.Length + saltBytes.Length, ivBytes, 0, ivBytes.Length);
            Array.Copy(encryptedBytes, iterationCountBytes.Length + saltBytes.Length + ivBytes.Length, encryptedTextBytes, 0, encryptedTextBytes.Length);

            int iterations = BitConverter.ToInt32(iterationCountBytes, 0);

            // Rfc2898DeriveBytes always uses UTF8 no BOM.
            using (Rfc2898DeriveBytes keyDerivationFunction = new Rfc2898DeriveBytes(secret, saltBytes, iterations))
            {
                byte[] keyBytes = keyDerivationFunction.GetBytes(KeyBitSize / 8);

                using (AesManaged aes = new AesManaged { KeySize = KeyBitSize, BlockSize = BlockBitSize })
                using (ICryptoTransform decryptor = aes.CreateDecryptor(keyBytes, ivBytes))
                using (MemoryStream memoryStream = new MemoryStream(encryptedTextBytes))
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                using (StreamReader streamReader = new StreamReader(cryptoStream, Encoding)) {
                    return streamReader.ReadToEnd();
                }
            }
        }

        // Explicit interface implementation.
        byte[] ICryptoKernel.Encrypt(string plainText, string secret)
        {
            return Encrypt(plainText, secret);
        }
    }
}