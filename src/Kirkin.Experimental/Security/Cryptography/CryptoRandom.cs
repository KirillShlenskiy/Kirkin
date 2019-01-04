using System.Security.Cryptography;

namespace Kirkin.Security.Cryptography
{
    internal static class CryptoRandom
    {
        internal static byte[] GetRandomBytes(int length)
        {
            byte[] bytes = new byte[length];

            // Reuse?
            using (RandomNumberGenerator rng = CryptoFactories.RngFactory()) {
                rng.GetBytes(bytes);
            }

            return bytes;
        }
    }
}