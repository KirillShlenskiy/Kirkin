using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Kirkin.IO;

using NUnit.Framework;

namespace Kirkin.Tests.IO
{
    public class StreamUtilTests
    {
        [Test]
        public Task ParallelCopyAsync1()
        {
            return ParallelCopyAsyncTest(1);
        }

        [Test]
        public Task ParallelCopyAsync2()
        {
            return ParallelCopyAsyncTest(2);
        }

        [Test]
        public Task ParallelCopyAsync3()
        {
            return ParallelCopyAsyncTest(3);
        }

        [Test]
        public Task ParallelCopyAsync5()
        {
            return ParallelCopyAsyncTest(5);
        }

        [Test]
        public Task ParallelCopyAsync8()
        {
            return ParallelCopyAsyncTest(8);
        }

        [Test]
        public Task ParallelCopyAsync9()
        {
            return ParallelCopyAsyncTest(9);
        }

        private async Task ParallelCopyAsyncTest(int bufferSize)
        {
            byte[] bytes = { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream source = new MemoryStream(bytes))
            using (MemoryStream target = new MemoryStream())
            {
                await StreamUtil.ParallelCopyAsync(source, target, bufferSize, CancellationToken.None);

                Assert.AreEqual(bytes, target.ToArray());
            }
        }
    }
}