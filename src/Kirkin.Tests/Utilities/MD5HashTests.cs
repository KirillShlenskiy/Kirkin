﻿using System.IO;
using System.Text;

using Kirkin.Utilities;

using Xunit;
using Xunit.Abstractions;

namespace Kirkin.Tests.Utilities
{
    public class MD5HashTests
    {
        private ITestOutputHelper Output;

        public MD5HashTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void Simple()
        {
            byte[] bytes = Encoding.UTF8.GetBytes("Hello world");
            MemoryStream stream = new MemoryStream(bytes);
            MD5Hash hash = MD5Hash.ComputeHash(stream);

            Output.WriteLine($"Hex: {hash}.");
            Output.WriteLine($"Base64: {hash.ToBase64String()}.");
        }
    }
}