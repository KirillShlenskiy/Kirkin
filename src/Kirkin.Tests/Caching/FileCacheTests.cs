using System;
using System.IO;
using System.Threading;

using Kirkin.Caching;
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

            ICache<Dummy> fileCache = new FileCache<Dummy>(
                () =>
                {
                    Interlocked.Increment(ref creationCount);

                    return new Dummy { ID = 123, Value = "Blah" };
                },
                filePath,
                new XmlSerializer()
            );

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

        [Fact]
        public void CacheCollection()
        {
            string filePath = @"C:\Temp\FileCacheTests\DummyArray.xml";

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            if (File.Exists(filePath)) {
                File.Delete(filePath);
            }

            int creationCount = 0;

            ICache<Dummy[]> fileCache = new FileCache<Dummy[]>(
                () =>
                {
                    Interlocked.Increment(ref creationCount);

                    return new[] {
                        new Dummy { ID = 123, Value = "Blah" }
                    };
                },
                filePath,
                new XmlSerializer()
            );

            Assert.Equal(1, fileCache.Value.Length);
        }

        [Fact]
        public void NonRoundtrippableCacheFails()
        {
            ICache<ImmutableDummy> cache = new FileCache<ImmutableDummy>(
                () => new ImmutableDummy(), "zzz", new XmlSerializer()
            );

            // ImmutableDummy cannot be cached because it has
            // properties with non-public setters, so deserialization
            // is guaranteed to fail. We're catching that early.
            Assert.Throws<InvalidOperationException>(() => cache.Value);
        }

        public class Dummy
        {
            public int ID { get; set; }
            public string Value { get; set; }
        }

        public class ImmutableDummy
        {
            public int ID { get; private set; }
            public string Value { get; private set; }
        }
    }
}