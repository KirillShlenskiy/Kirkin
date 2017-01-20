using Kirkin.Utilities;

using NUnit.Framework;

namespace Kirkin.Tests.Utilities
{
    public class MD5HashTests
    {
        [Test]
        public void Simple()
        {
            MD5Hash hash = MD5Hash.ComputeHash("Hello world");

            Output.WriteLine($"Hex: {hash}.");
            Output.WriteLine($"Base64: {hash.ToBase64String()}.");
        }
    }
}