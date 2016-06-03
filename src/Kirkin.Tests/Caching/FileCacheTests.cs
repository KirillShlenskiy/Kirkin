using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Kirkin.Caching;
using Kirkin.Caching.Persisted;
using Kirkin.Serialization;

using Xunit;

namespace Kirkin.Tests.Caching
{
    public class FileCacheTests
    {
        [Fact]
        public void BasicFunctionality()
        {
            string filePath = @"C:\Temp\FileCacheTests\Dummy.xml";

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            if (File.Exists(filePath)) {
                File.Delete(filePath);
            }

            int creationCount = 0;

            ICache<Dummy> rawCache = Cache.Uncached(() =>
            {
                Interlocked.Increment(ref creationCount);

                return new Dummy { ID = 123, Value = "Blah" };
            });

            ICache<Dummy> fileCache = new FileCache<Dummy>(rawCache, filePath, new XmlSerializer());

            Assert.False(fileCache.IsValid);

            Dummy d = fileCache.Value;

            Assert.Equal(123, d.ID);
            Assert.Equal("Blah", d.Value);
            Assert.Equal(1, creationCount);
            Assert.True(File.Exists(filePath));

            d = fileCache.Value;

            Assert.Equal(1, creationCount);

            File.Delete(filePath);

            d = fileCache.Value;

            Assert.Equal(2, creationCount);
        }

        public class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }
    }
}