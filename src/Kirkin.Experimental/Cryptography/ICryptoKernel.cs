namespace Kirkin.Cryptography
{
    /// <summary>
    /// Core encryptor/decryptor contract.
    /// </summary>
    public interface ICryptoKernel
    {
        /// <summary>
        /// Encrypts the given byte array using the specified secret.
        /// </summary>
        byte[] Encrypt(string plainText, string secret);

        /// <summary>
        /// Decrypts the given byte array using the specified secret.
        /// </summary>
        string Decrypt(byte[] encryptedBytes, string secret);
    }
}