using System;
using System.Security.Cryptography;

namespace Kirkin.Security.Cryptography
{
    internal static class CryptoFactories
    {
        // TODO: use Cng if targeting recent .NET/NETSTANDARD.
        internal static Func<Aes> AesFactory = new Func<Aes>(() => new AesCryptoServiceProvider());
        internal static Func<RandomNumberGenerator> RngFactory = new Func<RandomNumberGenerator>(() => new RNGCryptoServiceProvider());
    }
}