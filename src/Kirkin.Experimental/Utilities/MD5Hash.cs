using System;
using System.IO;
using System.Security.Cryptography;

namespace Kirkin.Utilities
{
    /// <summary>
    /// Encapsulates an MD5 hash.
    /// </summary>
    public sealed class MD5Hash
    {
        /// <summary>
        /// Computes the hash value for the file at the given path.
        /// </summary>
        public static MD5Hash ComputeFileHash(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath)) {
                return ComputeHash(stream);
            }
        }

        /// <summary>
        /// Computes the hash value for the given stream.
        /// </summary>
        public static MD5Hash ComputeHash(Stream stream)
        {
            // MD5 instances are not thread-safe,
            // so we create a new one on every call.
            using (MD5 algorithm = MD5.Create())
            {
                byte[] hashBytes = algorithm.ComputeHash(stream);

                return new MD5Hash(hashBytes);
            }
        }

        /// <summary>
        /// Byte array representation of the MD5 hash.
        /// </summary>
        public byte[] HashBytes { get; }

        /// <summary>
        /// Creates a new <see cref="MD5Hash"/> instance.
        /// </summary>
        private MD5Hash(byte[] hashBytes)
        {
            HashBytes = hashBytes;
        }

        /// <summary>
        /// Returns the hex representation of this instance.
        /// </summary>
        public override string ToString()
        {
            return BitConverter
                .ToString(HashBytes)
                .Replace("-", "")
                .ToLower();
        }

        /// <summary>
        /// Returns the Base64 representation of this string.
        /// </summary>
        /// <returns></returns>
        public string ToBase64String()
        {
            return Convert.ToBase64String(HashBytes);
        }
    }
}